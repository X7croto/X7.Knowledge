using Microsoft.CodeAnalysis;
using X7.Knowledge.Acquisition.Roslyn;
using X7.Knowledge.Model;

namespace X7.Knowledge.Compilation.Producers;

/// <summary>
/// C04 — herança e implementação, resolvidas semanticamente.
/// </summary>
/// <remarks>
/// Primeira capacidade que só existe em nível S. Em nível X estas relações
/// teriam de ser deduzidas por nome, o que §5.3 proíbe: o Producer declara a
/// limitação e não produz nada.
/// </remarks>
public sealed class TypeRelationProducer : IProducer
{
    /// <summary>
    /// Bases implícitas pelo próprio tipo do símbolo. Observá-las produziria
    /// uma Observation por tipo da solução sem informar nada: toda classe
    /// deriva de Object, todo struct de ValueType. Exclusão declarada, não
    /// omissão silenciosa — está documentada no catálogo do modelo.
    /// </summary>
    private static readonly SpecialType[] ImplicitBases =
    [
        SpecialType.System_Object,
        SpecialType.System_ValueType,
        SpecialType.System_Enum,
        SpecialType.System_Delegate,
        SpecialType.System_MulticastDelegate
    ];

    private readonly IReadOnlyList<SourceCompilation> _sources;

    public TypeRelationProducer(IReadOnlyList<SourceCompilation> sources)
        => _sources = sources;

    public string Name => nameof(TypeRelationProducer);

    public string Capability => "C04";

    public ValueTask ProduceAsync(
        CompilationContext context,
        CancellationToken cancellationToken)
    {
        // Nome do assembly para nome do projeto: o símbolo conhece o assembly,
        // e a identidade de tipo é ancorada no projeto.
        var projectByAssembly = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var source in _sources.Where(s => s.Compilation is not null))
        {
            var name = context.Solution.Projects
                .First(p => p.RelativePath == source.ProjectRelativePath)
                .Name;

            projectByAssembly[source.Compilation!.Assembly.Name] = name;
        }

        foreach (var source in _sources.OrderBy(s => s.ProjectRelativePath, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (source.Compilation is null)
            {
                context.Knowledge.Add(
                    ObservationKinds.AcquisitionLimitation,
                    KnowledgeId.ForProject(source.ProjectRelativePath),
                    ObservationPayload.From(
                        ("reason", "Herança e implementação exigem nível S; projeto lido apenas sintaticamente"),
                        ("affectedScope", "type-relations")),
                    new Provenance
                    {
                        Source = source.ProjectRelativePath,
                        Producer = Name,
                        Capability = Capability,
                        AcquisitionLevel = AcquisitionLevel.Syntactic
                    });

                continue;
            }

            Produce(context, source, projectByAssembly);
        }

        return ValueTask.CompletedTask;
    }

    private void Produce(
        CompilationContext context,
        SourceCompilation source,
        IReadOnlyDictionary<string, string> projectByAssembly)
    {
        var projectName = projectByAssembly[source.Compilation!.Assembly.Name];

        foreach (var type in SourceTypes(source.Compilation!.Assembly.GlobalNamespace)
                     .OrderBy(IdentityName, StringComparer.Ordinal))
        {
            var file = FileOf(source, type);

            if (file is null)
                continue;

            var typeId = KnowledgeId.ForType(IdentityName(type), projectName);

            var provenance = new Provenance
            {
                Source = file,
                Producer = Name,
                Capability = Capability,
                AcquisitionLevel = AcquisitionLevel.Semantic
            };

            EmitBase(context, type, typeId, provenance, projectByAssembly);
            EmitInterfaces(context, type, typeId, provenance, projectByAssembly);
        }
    }

    private void EmitBase(
        CompilationContext context,
        INamedTypeSymbol type,
        KnowledgeId typeId,
        Provenance provenance,
        IReadOnlyDictionary<string, string> projectByAssembly)
    {
        if (type.BaseType is not { } baseType)
            return;

        // Comparação pelo SpecialType, não pelo nome formatado:
        // FullyQualifiedFormat usa UseSpecialTypes e renderiza System.Object
        // como `object`, então comparar strings nunca casava e toda classe
        // ganhava uma relação inútil para `object`.
        if (ImplicitBases.Contains(baseType.SpecialType))
            return;

        context.Knowledge.Add(
            ObservationKinds.TypeInherits,
            typeId,
            Reference("baseTypeId", "baseTypeName", baseType, projectByAssembly),
            provenance);
    }

    private void EmitInterfaces(
        CompilationContext context,
        INamedTypeSymbol type,
        KnowledgeId typeId,
        Provenance provenance,
        IReadOnlyDictionary<string, string> projectByAssembly)
    {
        // Apenas as declaradas diretamente. O fecho transitivo é derivável a
        // partir destas: computá-lo aqui seria inferência disfarçada de
        // observação (OB-01).
        foreach (var contract in type.Interfaces
                     .OrderBy(DisplayName, StringComparer.Ordinal))
        {
            context.Knowledge.Add(
                ObservationKinds.TypeImplements,
                typeId,
                Reference("interfaceId", "interfaceName", contract, projectByAssembly),
                provenance);
        }
    }

    /// <summary>
    /// Tipo de fora da solução não vira identidade do modelo — não existe lá.
    /// Mas saber que algo deriva de `Exception` é conhecimento legítimo, então
    /// o nome é registrado em vez de a relação ser descartada.
    /// </summary>
    private static ObservationPayload Reference(
        string idKey,
        string nameKey,
        INamedTypeSymbol target,
        IReadOnlyDictionary<string, string> projectByAssembly)
    {
        // Nome exibido é o uso — `IFoo<List<string>>`. Identidade é a
        // declaração — `IFoo<T>`. São coisas diferentes: só a declaração
        // existe como tipo no modelo, e um mesmo genérico admite infinitas
        // instanciações. Confundir as duas quebra IV-13.
        var display = DisplayName(target);
        var identity = IdentityName(target);

        var assembly = target.OriginalDefinition.ContainingAssembly?.Name;

        if (assembly is not null && projectByAssembly.TryGetValue(assembly, out var project))
        {
            return ObservationPayload.From(
                (idKey, KnowledgeId.ForType(identity, project).Value),
                (nameKey, display));
        }

        return ObservationPayload.From(
            (nameKey, display),
            ("external", "true"));
    }

    private static IEnumerable<INamedTypeSymbol> SourceTypes(INamespaceOrTypeSymbol symbol)
    {
        foreach (var member in symbol.GetMembers())
        {
            switch (member)
            {
                case INamespaceSymbol child:
                    foreach (var found in SourceTypes(child))
                        yield return found;

                    break;

                case INamedTypeSymbol type:
                    yield return type;

                    foreach (var nested in SourceTypes(type))
                        yield return nested;

                    break;
            }
        }
    }

    /// <summary>Como o tipo aparece no código, com argumentos genéricos.</summary>
    private static string DisplayName(INamedTypeSymbol type)
        => Qualify(type);

    /// <summary>
    /// Como o tipo é identificado no modelo: sempre a definição original.
    /// `IFoo&lt;List&lt;string&gt;&gt;` e `IFoo&lt;int&gt;` são usos do mesmo
    /// tipo declarado `IFoo&lt;T&gt;`, e é ele que existe como identidade.
    /// </summary>
    private static string IdentityName(INamedTypeSymbol type)
        => Qualify(type.OriginalDefinition);

    private static string Qualify(INamedTypeSymbol type)
        => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
               .Replace("global::", string.Empty, StringComparison.Ordinal);

    private static string? FileOf(SourceCompilation source, INamedTypeSymbol type)
    {
        var path = type.Locations.FirstOrDefault(l => l.IsInSource)?.SourceTree?.FilePath;

        if (path is null)
            return null;

        return source.Files.FirstOrDefault(f => f.Tree.FilePath == path)?.RelativePath ?? path;
    }
}
