using X7.ProjectIndexer.CLI;
using X7.ProjectIndexer.Core.Services.Indexing;
using X7.ProjectIndexer.Knowledge;

var options = CommandLineParser.Parse(args);

Console.WriteLine($"Scanning solution '{options.Input}'...");

var indexer = new ProjectIndexer();

var index = indexer.Index(options.Input);

Console.WriteLine("Usings encontrados:");

//foreach (var project in index.Projects)
//{
//    foreach (var file in project.Files)
//    {
//        if (file.Usings.Count == 0)
//            continue;

//        Console.WriteLine();
//        Console.WriteLine(file.Path);

//        foreach (var u in file.Usings)
//        {
//            Console.WriteLine(
//                $"  using {u.Namespace}" +
//                $" alias={u.Alias}" +
//                $" static={u.IsStatic}" +
//                $" global={u.IsGlobal}");
//        }
//    }
//}

Console.WriteLine("Generating knowledge...");

new KnowledgeExporter().Export(index, options.Output);
Console.WriteLine();
Console.WriteLine("Done.");
Console.WriteLine(options.Output);