using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using X7.Knowledge.Acquisition;
using X7.Knowledge.Acquisition.Roslyn;
using X7.Knowledge.Model;

namespace X7.Knowledge.Compilation.Producers;

/// <summary>
/// C03 — organização lógica do código: namespaces, tipos e onde cada um mora.
/// Não representa comportamento, membros nem relações: isso é C04 em diante.
/// </summary>
public sealed class CodeStructureProducer : IProducer
{
    private readonly IReadOnlyList<SourceCompilation> _sources;

    public CodeStructureProducer(IReadOnlyList<SourceCompilation> sources)
        => _sources = sources;

    public string Name => nameof(CodeStructureProducer);

    public string Capability => "C03";

    public ValueTask ProduceAsync(
        CompilationContext context,
        CancellationToken cancellationToken)
    {
        var namespaces = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var source in _sources.OrderBy(s => s.ProjectRelativePath, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var projectId = KnowledgeId.ForProject(source.ProjectRelativePath);

            var projectName = context.Solution.Projects
                .First(p => p.RelativePath == source.ProjectRelativePath)
                .Name;

            foreach (var limitation in source.Limitations)
            {
                context.Knowledge.Add(
                    ObservationKinds.AcquisitionLimitation,
                    projectId,
                    ObservationPayload.From(
                        ("reason", limitation.Reason),
                        ("affectedScope", limitation.AffectedScope)),
                    new Provenance
                    {
                        Source = limitation.Source,
                        Producer = Name,
                        Capability = Capability,
                        AcquisitionLevel = source.Level
                    });
            }

            var declarations = source.Compilation is null
                ? FromSyntax(source)
                : FromSemantics(source);

            foreach (var declaration in declarations
                         .OrderBy(d => d.MetadataName, StringComparer.Ordinal)
                         .ThenBy(d => d.File, StringComparer.Ordinal))
            {
                Emit(context, source, projectId, projectName, declaration, namespaces);
            }
        }

        EmitNamespaces(context, namespaces);

        return ValueTask.CompletedTask;
    }

    private sealed record Declaration
    {
        public required string MetadataName { get; init; }

        public required string Namespace { get; init; }

        public required string Name { get; init; }

        public required string File { get; init; }
    }

    private void Emit(
        CompilationContext context,
        SourceCompilation source,
        KnowledgeId projectId,
        string projectName,
        Declaration declaration,
        SortedSet<string> namespaces)
    {
        var typeId = KnowledgeId.ForType(declaration.MetadataName, projectName);

        var provenance = new Provenance
        {
            Source = declaration.File,
            Producer = Name,
            Capability = Capability,
            AcquisitionLevel = source.Level
        };

        context.Knowledge.Add(
            ObservationKinds.TypeDeclared,
            typeId,
            ObservationPayload.From(
                ("name", declaration.Name),
                ("metadataName", declaration.MetadataName),
                ("namespace", declaration.Namespace.Length == 0 ? null : declaration.Namespace),
                ("projectId", projectId.Value)),
            provenance);

        context.Knowledge.Add(
            ObservationKinds.TypeLocation,
            typeId,
            ObservationPayload.From(("file", declaration.File)),
            provenance);

        if (declaration.Namespace.Length == 0)
            return;

        namespaces.Add(declaration.Namespace);

        context.Knowledge.Add(
            ObservationKinds.NamespaceContains,
            KnowledgeId.ForNamespace(declaration.Namespace),
            ObservationPayload.From(("typeId", typeId.Value)),
            provenance);
    }

    /// <summary>Namespace declarado e sua hierarquia. Pai é o prefixo.</summary>
    private void EmitNamespaces(CompilationContext context, SortedSet<string> namespaces)
    {
        var complete = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var name in namespaces)
        {
            var segments = name.Split('.');

            for (var i = 1; i <= segments.Length; i++)
                complete.Add(string.Join('.', segments.Take(i)));
        }

        foreach (var name in complete)
        {
            var index = name.LastIndexOf('.');

            var parent = index < 0 ? null : name[..index];

            context.Knowledge.Add(
                ObservationKinds.NamespaceDeclared,
                KnowledgeId.ForNamespace(name),
                ObservationPayload.From(
                    ("name", name),
                    ("parentId", parent is null ? null : KnowledgeId.ForNamespace(parent).Value)),
                new Provenance
                {
                    Source = context.Solution.FileName,
                    Producer = Name,
                    Capability = Capability,
                    AcquisitionLevel = context.AcquisitionLevel
                });
        }
    }

    /// <summary>Nível S: nomes vêm de símbolos resolvidos.</summary>
    private static IEnumerable<Declaration> FromSemantics(SourceCompilation source)
    {
        var results = new List<Declaration>();

        void Walk(INamespaceOrTypeSymbol symbol)
        {
            foreach (var member in symbol.GetMembers())
            {
                switch (member)
                {
                    case INamespaceSymbol child:
                        Walk(child);
                        break;

                    case INamedTypeSymbol type:
                        var location = type.Locations.FirstOrDefault(l => l.IsInSource);

                        if (location?.SourceTree?.FilePath is not { } path)
                            continue;

                        results.Add(new Declaration
                        {
                            MetadataName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                                .Replace("global::", string.Empty, StringComparison.Ordinal),
                            Namespace = type.ContainingNamespace.IsGlobalNamespace
                                ? string.Empty
                                : type.ContainingNamespace.ToDisplayString(),
                            Name = type.MetadataName,
                            File = source.Files
                                .FirstOrDefault(f => f.Tree.FilePath == path)?.RelativePath
                                   ?? path
                        });

                        Walk(type);
                        break;
                }
            }
        }

        Walk(source.Compilation!.Assembly.GlobalNamespace);

        return results;
    }

    /// <summary>
    /// Nível X: nomes vêm da árvore sintática. Namespace é composto pelos
    /// ancestrais; nada é resolvido. Fiel ao limite declarado do nível.
    /// </summary>
    private static IEnumerable<Declaration> FromSyntax(SourceCompilation source)
    {
        foreach (var file in source.Files)
        {
            foreach (var node in file.Tree.GetRoot().DescendantNodes()
                         .Where(n => n is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax))
            {
                var name = node switch
                {
                    BaseTypeDeclarationSyntax t => t.Identifier.ValueText,
                    DelegateDeclarationSyntax d => d.Identifier.ValueText,
                    _ => null
                };

                if (name is null)
                    continue;

                // Namespaces e tipos aninhados são acumulados em separado:
                // o namespace do tipo é só a parte de namespace.
                var namespaces = new List<string>();
                var outerTypes = new List<string>();

                for (var current = node.Parent; current is not null; current = current.Parent)
                {
                    switch (current)
                    {
                        case BaseNamespaceDeclarationSyntax ns:
                            namespaces.Insert(0, ns.Name.ToString());
                            break;

                        case BaseTypeDeclarationSyntax outer:
                            outerTypes.Insert(0, outer.Identifier.ValueText);
                            break;
                    }
                }

                var namespaceName = string.Join('.', namespaces);

                var qualifiedName = string.Join('.', outerTypes.Append(name));

                yield return new Declaration
                {
                    MetadataName = namespaceName.Length == 0
                        ? qualifiedName
                        : $"{namespaceName}.{qualifiedName}",
                    Namespace = namespaceName,
                    Name = name,
                    File = file.RelativePath
                };
            }
        }
    }
}
