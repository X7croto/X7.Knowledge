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
                         .OrderBy(d => d.MetadataName, StringComparer.Ordinal))
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

        /// <summary>
        /// Todos os arquivos onde o tipo é declarado, ordenados. Tipo parcial
        /// tem mais de um, e registrar apenas o primeiro apagaria o fato — é
        /// dele que a Inference `type.is-partial` deriva.
        /// </summary>
        public required IReadOnlyList<string> Files { get; init; }

        /// <summary>
        /// Tipo aninhado não é conteúdo direto do namespace: quem o contém é
        /// o tipo externo. Ver `type.nested-in` (C04).
        /// </summary>
        public required bool IsNested { get; init; }
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
            Source = declaration.Files[0],
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

        foreach (var file in declaration.Files)
        {
            context.Knowledge.Add(
                ObservationKinds.TypeLocation,
                typeId,
                ObservationPayload.From(("file", file)),
                provenance with { Source = file });
        }

        if (declaration.Namespace.Length == 0)
            return;

        namespaces.Add(declaration.Namespace);

        // Tipo aninhado tem o namespace do contentor, mas não é conteúdo
        // direto dele: publicar os dois daria dois caminhos até o mesmo tipo
        // e a hierarquia deixaria de ser árvore.
        if (declaration.IsNested)
            return;

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
                        var files = type.Locations
                            .Where(l => l.IsInSource)
                            .Select(l => l.SourceTree?.FilePath)
                            .OfType<string>()
                            // Arquivo fora do conjunto observado é fora da
                            // fronteira (ADR-041). O `?? path` que havia aqui
                            // publicava o caminho absoluto da máquina, e a
                            // IV-08 não o reconhecia depois de normalizado.
                            .Select(path => source.Files
                                .FirstOrDefault(f => f.Tree.FilePath == path)?.RelativePath)
                            .OfType<string>()
                            .Distinct(StringComparer.Ordinal)
                            .OrderBy(f => f, StringComparer.Ordinal)
                            .ToArray();

                        if (files.Length == 0)
                            continue;

                        results.Add(new Declaration
                        {
                            // OriginalDefinition explícito: a identidade de um
                            // genérico é sempre a declaração, nunca uma
                            // instanciação. C04 depende dessa mesma regra.
                            MetadataName = TypeIdentity.Semantic(type),
                            Namespace = type.ContainingNamespace.IsGlobalNamespace
                                ? string.Empty
                                : type.ContainingNamespace.ToDisplayString(),
                            Name = type.MetadataName,
                            Files = files,
                            IsNested = type.ContainingType is not null
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
        // Agrupado por nome: um tipo parcial aparece em vários arquivos e é
        // um tipo só. Sem o agrupamento, o mesmo tipo entraria duas vezes e a
        // contagem da Base mentiria.
        var byName = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var declarations = new Dictionary<string, Declaration>(StringComparer.Ordinal);

        foreach (var file in source.Files)
        {
            foreach (var node in file.Tree.GetRoot().DescendantNodes()
                         .Where(n => n is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax))
            {
                var identity = TypeIdentity.Syntactic(node);

                if (!byName.TryGetValue(identity.MetadataName, out var files))
                {
                    files = [];
                    byName[identity.MetadataName] = files;

                    declarations[identity.MetadataName] = new Declaration
                    {
                        MetadataName = identity.MetadataName,
                        Namespace = identity.Namespace,
                        Name = identity.Name,
                        Files = files,
                        IsNested = TypeIdentity.ContainerOf(node) is not null
                    };
                }

                if (!files.Contains(file.RelativePath, StringComparer.Ordinal))
                    files.Add(file.RelativePath);
            }
        }

        foreach (var files in byName.Values)
            files.Sort(StringComparer.Ordinal);

        return declarations.Values;
    }
}
