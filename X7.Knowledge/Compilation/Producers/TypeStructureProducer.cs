using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using X7.Knowledge.Acquisition.Roslyn;
using X7.Knowledge.Model;

namespace X7.Knowledge.Compilation.Producers;

/// <summary>
/// C04 — estrutura declarada de cada tipo: classificação, acessibilidade,
/// modificadores, parâmetros genéricos e aninhamento.
/// </summary>
/// <remarks>
/// Ao contrário de herança e implementação, nada aqui exige nível S: tudo
/// está na declaração. Por isso o Producer opera nos dois níveis e não
/// declara limitação por nível. Se produzisse apenas em S, IV-14 falharia em
/// nível X sobre tipos que o C03 declarou, e o C04 quebraria uma capacidade
/// anterior (PL-06).
/// </remarks>
public sealed class TypeStructureProducer : IProducer
{
    private readonly IReadOnlyList<SourceCompilation> _sources;

    public TypeStructureProducer(IReadOnlyList<SourceCompilation> sources)
        => _sources = sources;

    public string Name => nameof(TypeStructureProducer);

    public string Capability => "C04";

    public ValueTask ProduceAsync(
        CompilationContext context,
        CancellationToken cancellationToken)
    {
        foreach (var source in _sources.OrderBy(s => s.ProjectRelativePath, StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var projectName = context.Solution.Projects
                .First(p => p.RelativePath == source.ProjectRelativePath)
                .Name;

            if (source.Compilation is null)
                FromSyntax(context, source, projectName);
            else
                FromSemantics(context, source, projectName);
        }

        return ValueTask.CompletedTask;
    }

    // ---------------------------------------------------------------- nível S

    private void FromSemantics(
        CompilationContext context,
        SourceCompilation source,
        string projectName)
    {
        foreach (var type in SourceTypes(source.Compilation!.Assembly.GlobalNamespace)
                     .OrderBy(TypeIdentity.Semantic, StringComparer.Ordinal))
        {
            var file = FileOf(source, type);

            if (file is null)
                continue;

            var typeId = KnowledgeId.ForType(TypeIdentity.Semantic(type), projectName);

            var provenance = new Provenance
            {
                Source = file,
                Producer = Name,
                Capability = Capability,
                AcquisitionLevel = AcquisitionLevel.Semantic
            };

            Emit(context, typeId, provenance, KindOf(type), AccessibilityOf(type));

            // Modificadores vêm da declaração, mesmo em nível S. O símbolo
            // expõe a forma de metadados, não a declarada: toda interface é
            // `IsAbstract`, todo enum é `IsSealed`, e classe estática é as
            // duas coisas. Publicar isso encheria a Base de modificador que
            // ninguém escreveu — o mesmo problema das bases implícitas.
            foreach (var modifier in DeclaredModifiers(type))
                EmitModifier(context, typeId, provenance, modifier);

            foreach (var parameter in type.TypeParameters.OrderBy(p => p.Ordinal))
            {
                EmitGenericParameter(
                    context,
                    typeId,
                    provenance,
                    parameter.Name,
                    parameter.Ordinal,
                    VarianceOf(parameter.Variance));
            }

            if (type.ContainingType is { } container)
            {
                EmitNestedIn(
                    context,
                    typeId,
                    provenance,
                    KnowledgeId.ForType(TypeIdentity.Semantic(container), projectName));
            }
        }
    }

    private static string KindOf(INamedTypeSymbol type) => type switch
    {
        { TypeKind: Microsoft.CodeAnalysis.TypeKind.Interface } => TypeVocabulary.Interface,
        { TypeKind: Microsoft.CodeAnalysis.TypeKind.Enum } => TypeVocabulary.Enum,
        { TypeKind: Microsoft.CodeAnalysis.TypeKind.Delegate } => TypeVocabulary.Delegate,
        { TypeKind: Microsoft.CodeAnalysis.TypeKind.Struct, IsRecord: true } => TypeVocabulary.RecordStruct,
        { TypeKind: Microsoft.CodeAnalysis.TypeKind.Struct } => TypeVocabulary.Struct,
        { IsRecord: true } => TypeVocabulary.Record,
        _ => TypeVocabulary.Class
    };

    private static string AccessibilityOf(INamedTypeSymbol type) => type.DeclaredAccessibility switch
    {
        Accessibility.Public => TypeVocabulary.Public,
        Accessibility.Protected => TypeVocabulary.Protected,
        Accessibility.Private => TypeVocabulary.Private,
        Accessibility.ProtectedOrInternal => TypeVocabulary.ProtectedInternal,
        Accessibility.ProtectedAndInternal => TypeVocabulary.PrivateProtected,
        _ => TypeVocabulary.Internal
    };

    private static IEnumerable<string> DeclaredModifiers(INamedTypeSymbol type)
    {
        var modifiers = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var reference in type.DeclaringSyntaxReferences)
        {
            foreach (var modifier in ModifiersOf(reference.GetSyntax()))
                modifiers.Add(modifier);
        }

        return modifiers;
    }

    // ---------------------------------------------------------------- nível X

    private void FromSyntax(
        CompilationContext context,
        SourceCompilation source,
        string projectName)
    {
        // Agrupado por identidade antes de emitir. Um tipo parcial pode
        // declarar a acessibilidade em um arquivo e omiti-la no outro — é C#
        // válido. Emitindo por nó, o mesmo tipo receberia `public` de uma
        // declaração e o padrão `internal` da outra, e IV-14 abortaria a
        // compilação por culpa do observador, não do código observado.
        var declarations = new SortedDictionary<string, SyntacticType>(StringComparer.Ordinal);

        foreach (var file in source.Files)
        {
            foreach (var node in file.Tree.GetRoot().DescendantNodes()
                         .Where(n => n is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax))
            {
                var identity = TypeIdentity.Syntactic(node);

                if (!declarations.TryGetValue(identity.MetadataName, out var declaration))
                {
                    declaration = new SyntacticType
                    {
                        Kind = SyntacticKindOf(node),
                        File = file.RelativePath,
                        DefaultAccessibility = DefaultAccessibilityOf(node),
                        Container = TypeIdentity.ContainerOf(node) is { } container
                            ? TypeIdentity.Syntactic(container).MetadataName
                            : null
                    };

                    declarations[identity.MetadataName] = declaration;
                }

                declaration.Accessibility ??= DeclaredAccessibilityOf(node);

                foreach (var modifier in ModifiersOf(node))
                    declaration.Modifiers.Add(modifier);

                var parameters = node switch
                {
                    TypeDeclarationSyntax t => t.TypeParameterList,
                    DelegateDeclarationSyntax d => d.TypeParameterList,
                    _ => null
                };

                if (parameters is null || declaration.Parameters.Count > 0)
                    continue;

                for (var i = 0; i < parameters.Parameters.Count; i++)
                {
                    var parameter = parameters.Parameters[i];

                    var variance = parameter.VarianceKeyword.ValueText;

                    declaration.Parameters.Add(new GenericParameter
                    {
                        Name = parameter.Identifier.ValueText,
                        Ordinal = i,
                        Variance = variance.Length == 0 ? null : variance
                    });
                }
            }
        }

        foreach (var (metadataName, declaration) in declarations)
        {
            var typeId = KnowledgeId.ForType(metadataName, projectName);

            var provenance = new Provenance
            {
                Source = declaration.File,
                Producer = Name,
                Capability = Capability,
                AcquisitionLevel = AcquisitionLevel.Syntactic
            };

            Emit(
                context,
                typeId,
                provenance,
                declaration.Kind,
                declaration.Accessibility ?? declaration.DefaultAccessibility);

            foreach (var modifier in declaration.Modifiers)
                EmitModifier(context, typeId, provenance, modifier);

            foreach (var parameter in declaration.Parameters)
            {
                EmitGenericParameter(
                    context,
                    typeId,
                    provenance,
                    parameter.Name,
                    parameter.Ordinal,
                    parameter.Variance);
            }

            if (declaration.Container is { } container)
            {
                EmitNestedIn(
                    context,
                    typeId,
                    provenance,
                    KnowledgeId.ForType(container, projectName));
            }
        }
    }

    private sealed class SyntacticType
    {
        public required string Kind { get; init; }

        public required string File { get; init; }

        public required string? Container { get; init; }

        /// <summary>Padrão da linguagem, quando nenhuma declaração informa.</summary>
        public required string DefaultAccessibility { get; init; }

        public string? Accessibility { get; set; }

        public SortedSet<string> Modifiers { get; } = new(StringComparer.Ordinal);

        public List<GenericParameter> Parameters { get; } = [];
    }

    private sealed record GenericParameter
    {
        public required string Name { get; init; }

        public required int Ordinal { get; init; }

        public required string? Variance { get; init; }
    }

    private static string SyntacticKindOf(SyntaxNode node) => node switch
    {
        RecordDeclarationSyntax record
            => record.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword)
                ? TypeVocabulary.RecordStruct
                : TypeVocabulary.Record,
        InterfaceDeclarationSyntax => TypeVocabulary.Interface,
        StructDeclarationSyntax => TypeVocabulary.Struct,
        EnumDeclarationSyntax => TypeVocabulary.Enum,
        DelegateDeclarationSyntax => TypeVocabulary.Delegate,
        _ => TypeVocabulary.Class
    };

    /// <summary>Acessibilidade escrita na declaração, ou nada quando omitida.</summary>
    private static string? DeclaredAccessibilityOf(SyntaxNode node)
    {
        var names = RawModifiers(node)
            .Select(m => m.ValueText)
            .ToHashSet(StringComparer.Ordinal);

        if (names.Contains("protected"))
        {
            return names.Contains("internal")
                ? TypeVocabulary.ProtectedInternal
                : names.Contains("private")
                    ? TypeVocabulary.PrivateProtected
                    : TypeVocabulary.Protected;
        }

        if (names.Contains("public"))
            return TypeVocabulary.Public;

        if (names.Contains("internal"))
            return TypeVocabulary.Internal;

        return names.Contains("private") ? TypeVocabulary.Private : null;
    }

    /// <summary>
    /// Padrão da linguagem quando a acessibilidade é omitida: `internal` no
    /// nível superior, `private` dentro de tipo, `public` dentro de interface.
    /// </summary>
    private static string DefaultAccessibilityOf(SyntaxNode node)
        => TypeIdentity.ContainerOf(node) switch
        {
            InterfaceDeclarationSyntax => TypeVocabulary.Public,
            not null => TypeVocabulary.Private,
            null => TypeVocabulary.Internal
        };

    // ------------------------------------------------------------- comum

    private static SyntaxTokenList RawModifiers(SyntaxNode node) => node switch
    {
        BaseTypeDeclarationSyntax t => t.Modifiers,
        DelegateDeclarationSyntax d => d.Modifiers,
        _ => default
    };

    private static IEnumerable<string> ModifiersOf(SyntaxNode node)
        => RawModifiers(node)
            .Select(m => m.ValueText)
            .Where(TypeVocabulary.IsKnownModifier);

    private static string? VarianceOf(VarianceKind variance) => variance switch
    {
        VarianceKind.In => "in",
        VarianceKind.Out => "out",
        _ => null
    };

    private void Emit(
        CompilationContext context,
        KnowledgeId typeId,
        Provenance provenance,
        string kind,
        string accessibility)
    {
        if (!TypeVocabulary.IsKnownKind(kind))
            throw new InvalidOperationException($"Classificação fora do vocabulário: '{kind}'.");

        if (!TypeVocabulary.IsKnownAccessibility(accessibility))
            throw new InvalidOperationException($"Acessibilidade fora do vocabulário: '{accessibility}'.");

        context.Knowledge.Add(
            ObservationKinds.TypeKind,
            typeId,
            ObservationPayload.From(("kind", kind)),
            provenance);

        context.Knowledge.Add(
            ObservationKinds.TypeAccessibility,
            typeId,
            ObservationPayload.From(("value", accessibility)),
            provenance);
    }

    private void EmitModifier(
        CompilationContext context,
        KnowledgeId typeId,
        Provenance provenance,
        string modifier)
        => context.Knowledge.Add(
            ObservationKinds.TypeModifier,
            typeId,
            ObservationPayload.From(("name", modifier)),
            provenance);

    /// <summary>
    /// O ordinal está no payload, e não implícito na ordem da coleção: D-01
    /// ordena a saída por identidade canônica, o que embaralharia
    /// `&lt;TKey, TValue&gt;`. A ordem de declaração é semântica e precisa
    /// sobreviver dentro do próprio fato.
    /// </summary>
    private void EmitGenericParameter(
        CompilationContext context,
        KnowledgeId typeId,
        Provenance provenance,
        string name,
        int ordinal,
        string? variance)
        => context.Knowledge.Add(
            ObservationKinds.TypeGenericParameter,
            typeId,
            ObservationPayload.From(
                ("name", name),
                ("ordinal", ordinal.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                ("variance", variance)),
            provenance);

    private void EmitNestedIn(
        CompilationContext context,
        KnowledgeId typeId,
        Provenance provenance,
        KnowledgeId containerId)
        => context.Knowledge.Add(
            ObservationKinds.TypeNestedIn,
            typeId,
            ObservationPayload.From(("containerId", containerId.Value)),
            provenance);

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

    private static string? FileOf(SourceCompilation source, INamedTypeSymbol type)
    {
        var path = type.Locations.FirstOrDefault(l => l.IsInSource)?.SourceTree?.FilePath;

        if (path is null)
            return null;

        return source.Files.FirstOrDefault(f => f.Tree.FilePath == path)?.RelativePath ?? path;
    }
}
