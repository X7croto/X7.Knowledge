namespace X7.ProjectIndexer.Core.Integrity.Rules;

using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Symbols;

public sealed class BrokenReferenceRule : IntegrityValidationRule
{
    public override string Category => "Graph";

    protected override void Execute(
        ProjectIndexOld index,
        IntegrityValidationContext context)
    {
        ValidateTypes(index, context);
        ValidateMethods(index, context);
        ValidateProperties(index, context);
        ValidateFields(index, context);
        ValidateCalls(index, context);
    }

    private static void ValidateTypes(
        ProjectIndexOld index,
        IntegrityValidationContext context)
    {
        foreach (var type in index.Semantic.Types)
        {
            // NÃO assumir BaseTypeId (não existe no modelo real)

            if (type.BaseType is null)
            {
                // só valida se existe tentativa de resolução via string em outro campo
                // como não temos BaseTypeId real, não podemos validar nome

                continue;
            }

            foreach (var iface in type.Interfaces)
            {
                if (iface is null)
                {
                    context.Error(
                        "IDX101",
                        $"Type '{type.Name}' has unresolved interface reference.");
                }
            }
        }
    }
    private static void ValidateMethods(
        ProjectIndexOld index,
        IntegrityValidationContext context)
    {
        foreach (var method in index.Semantic.Methods)
        {
            if (method.DeclaringType is null)
            {
                context.Error(
                    "IDX102",
                    $"Method '{method.Name}' is not attached to a type.");
            }

            if (method.ReturnTypeSymbol is null &&
                method.ReturnType != "void")
            {
                context.Error(
                    "IDX103",
                    $"Method '{method.Name}' has unresolved return type '{method.ReturnType}'.");
            }
        }
    }

    private static void ValidateProperties(
        ProjectIndexOld index,
        IntegrityValidationContext context)
    {
        foreach (var property in index.Semantic.Properties)
        {
            if (property.TypeSymbol is null)
            {
                context.Error(
                    "IDX104",
                    $"Property '{property.Name}' has unresolved type '{property.Type}'.");
            }

            if (property.DeclaringType is null)
            {
                context.Error(
                    "IDX105",
                    $"Property '{property.Name}' is not attached to a type.");
            }
        }
    }

    private static void ValidateFields(
        ProjectIndexOld index,
        IntegrityValidationContext context)
    {
        foreach (var field in index.Semantic.Fields)
        {
            if (field.TypeSymbol is null)
            {
                context.Error(
                    "IDX106",
                    $"Field '{field.Name}' has unresolved type '{field.Type}'.");
            }

            if (field.DeclaringType is null)
            {
                context.Error(
                    "IDX107",
                    $"Field '{field.Name}' is not attached to a type.");
            }
        }
    }

    private static void ValidateCalls(
        ProjectIndexOld index,
        IntegrityValidationContext context)
    {
        foreach (var method in index.Semantic.Methods)
        {
            foreach (var callee in method.Callees)
            {
                if (!callee.Callers.Contains(method))
                {
                    context.Error(
                        "IDX108",
                        $"Call graph inconsistency: '{method.Name}' -> '{callee.Name}'.");
                }
            }
        }
    }
}