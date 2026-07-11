using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Binding;

public static class BindingDiagnostics
{
    public static void Print(ProjectIndexOld index)
    {
        int total = 0;
        int resolved = 0;
        int unresolved = 0;
        int ambiguous = 0;

        foreach (var type in index.Projects
                     .SelectMany(x => x.Files)
                     .SelectMany(x => x.Types))
        {
            Count(type.BaseTypeReference);

            foreach (var i in type.InterfaceReferences)
                Count(i);

            foreach (var field in type.Fields)
                Count(field.TypeReference);

            foreach (var property in type.Properties)
                Count(property.TypeReference);

            foreach (var method in type.Methods)
            {
                Count(method.ReturnTypeReference);

                foreach (var parameter in method.Parameters)
                    Count(parameter.TypeReference);

                foreach (var local in method.Body.LocalVariables)
                    Count(local.TypeReference);
            }
        }

        Console.WriteLine();
        Console.WriteLine("========== BINDER ==========");

        Console.WriteLine($"Total      : {total}");
        Console.WriteLine($"Resolved   : {resolved}");
        Console.WriteLine($"Ambiguous  : {ambiguous}");
        Console.WriteLine($"Unresolved : {unresolved}");

        Console.WriteLine("============================");
        Console.WriteLine();

        void Count(TypeReference? reference)
        {
            if (reference is null)
                return;

            total++;

            if (reference.Ambiguous)
            {
                ambiguous++;
                return;
            }

            if (reference.Resolved)
            {
                resolved++;
                return;
            }

            unresolved++;
        }

        PrintUnresolved(index);
        PrintExamples(index);
    }

    private static void PrintUnresolved(ProjectIndexOld index)
    {
        var unresolved =
            new Dictionary<string, int>();

        void Add(TypeReference? reference)
        {
            if (reference is null)
                return;

            if (reference.Resolved)
                return;

            unresolved.TryAdd(reference.OriginalText, 0);

            unresolved[reference.OriginalText]++;
        }

        foreach (var type in index.Projects
                         .SelectMany(x => x.Files)
                         .SelectMany(x => x.Types))
        {
            Add(type.BaseTypeReference);

            foreach (var i in type.InterfaceReferences)
                Add(i);

            foreach (var field in type.Fields)
                Add(field.TypeReference);

            foreach (var property in type.Properties)
                Add(property.TypeReference);

            foreach (var method in type.Methods)
            {
                Add(method.ReturnTypeReference);

                foreach (var parameter in method.Parameters)
                    Add(parameter.TypeReference);

                foreach (var local in method.Body.LocalVariables)
                    Add(local.TypeReference);
            }
        }

        Console.WriteLine("===== TOP UNRESOLVED =====");

        foreach (var item in unresolved
                     .OrderByDescending(x => x.Value)
                     .Take(30))
        {
            Console.WriteLine($"{item.Key,-40} {item.Value}");
        }

        Console.WriteLine();
    }

    private static void PrintExamples(ProjectIndexOld index)
    {
        Console.WriteLine("===== FIRST UNRESOLVED =====");

        foreach (var type in index.Projects
                         .SelectMany(x => x.Files)
                         .SelectMany(x => x.Types))
        {
            foreach (var field in type.Fields)
            {
                if (field.TypeReference is null)
                    continue;

                if (field.TypeReference.Resolved)
                    continue;   

                Console.WriteLine();

                Console.WriteLine(field.TypeReference.OriginalText);

                Console.WriteLine($"Namespace : {type.Namespace}");

                Console.WriteLine($"Type      : {type.Name}");

                Console.WriteLine($"Field     : {field.Name}");

                break;
            }
        }

        Console.WriteLine();
    }
}