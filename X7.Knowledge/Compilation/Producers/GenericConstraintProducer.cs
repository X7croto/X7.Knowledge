using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using X7.Knowledge.Acquisition.Roslyn;
using X7.Knowledge.Model;

namespace X7.Knowledge.Compilation.Producers;

/// <summary>
/// C05, terceira fatia — restrições de parâmetro genérico, de tipo e de
/// membro (ADR-043).
/// </summary>
/// <remarks>
/// Producer próprio, e não uma extensão do `TypeStructureProducer`, porque a
/// restrição é conhecimento do C05 e aquele Producer declara C04. Provenance
/// carrega a capacidade, e o corte por `--until` depende dela.
///
/// As restrições vêm da sintaxe pelo motivo já estabelecido na fatia A para
/// modificadores: o símbolo expõe a forma de metadados e não preserva a ordem
/// escrita. O `typeId` das restrições de tipo sai do símbolo, caminhando
/// `ConstraintTypes` na mesma ordem em que as restrições de tipo aparecem
/// escritas.
/// </remarks>
public sealed class GenericConstraintProducer : IProducer
{
    private readonly IReadOnlyList<SourceCompilation> _sources;

    public GenericConstraintProducer(IReadOnlyList<SourceCompilation> sources)
        => _sources = sources;

    public string Name => nameof(GenericConstraintProducer);

    public string Capability => "C05";

    public ValueTask ProduceAsync(
        CompilationContext context,
        CancellationToken cancellationToken)
    {
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
                continue;

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
                     .OrderBy(TypeIdentity.Semantic, StringComparer.Ordinal))
        {
            var file = FileOf(source, type);

            if (file is null)
                continue;

            var provenance = new Provenance
            {
                Source = file,
                Producer = Name,
                Capability = Capability,
                AcquisitionLevel = AcquisitionLevel.Semantic
            };

            if (type.TypeParameters.Length > 0)
            {
                Emit(
                    context,
                    ObservationKinds.TypeGenericConstraint,
                    KnowledgeId.ForType(TypeIdentity.Semantic(type), projectName),
                    type,
                    type.TypeParameters,
                    provenance,
                    projectByAssembly);
            }

            var generics = type.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => !m.IsImplicitlyDeclared && m.TypeParameters.Length > 0)
                .OrderBy(MemberIdentity.Semantic, StringComparer.Ordinal);

            foreach (var method in generics)
            {
                var declaredIn = FileOf(source, method);

                if (declaredIn is null)
                    continue;

                Emit(
                    context,
                    ObservationKinds.MemberGenericConstraint,
                    KnowledgeId.ForMember(MemberIdentity.Semantic(method), projectName),
                    method,
                    method.TypeParameters,
                    provenance with { Source = declaredIn },
                    projectByAssembly);
            }
        }
    }

    private void Emit(
        CompilationContext context,
        string kind,
        KnowledgeId subject,
        ISymbol declaring,
        IReadOnlyList<ITypeParameterSymbol> parameters,
        Provenance provenance,
        IReadOnlyDictionary<string, string> projectByAssembly)
    {
        foreach (var clause in Clauses(declaring))
        {
            var parameterName = clause.Name.Identifier.ValueText;

            var parameter = parameters.FirstOrDefault(p =>
                string.Equals(p.Name, parameterName, StringComparison.Ordinal));

            ITypeSymbol[] constraintTypes = parameter is null
                ? []
                : parameter.ConstraintTypes.ToArray();

            var typeIndex = 0;
            var ordinal = 0;

            foreach (var constraint in clause.Constraints)
            {
                var form = MemberVocabulary.KeywordConstraint;
                var value = Normalize(constraint.ToString());
                ITypeSymbol? target = null;

                if (constraint is TypeConstraintSyntax written && !IsKeyword(written, parameter))
                {
                    target = typeIndex < constraintTypes.Length
                        ? constraintTypes[typeIndex]
                        : null;

                    typeIndex++;

                    form = target is ITypeParameterSymbol
                        ? MemberVocabulary.TypeParameterConstraint
                        : MemberVocabulary.TypeConstraint;

                    if (target is not null)
                        value = TypeIdentity.Display(target);
                }

                if (!MemberVocabulary.IsKnownConstraintForm(form))
                    throw new InvalidOperationException($"Forma de restrição fora do vocabulário: '{form}'.");

                context.Knowledge.Add(
                    kind,
                    subject,
                    Payload(parameterName, ordinal, form, value, target, projectByAssembly),
                    provenance);

                ordinal++;
            }
        }
    }

    private static ObservationPayload Payload(
        string parameter,
        int ordinal,
        string form,
        string value,
        ITypeSymbol? target,
        IReadOnlyDictionary<string, string> projectByAssembly)
    {
        string? typeId = null;
        string? external = null;

        if (target is INamedTypeSymbol named)
        {
            var assembly = named.OriginalDefinition.ContainingAssembly?.Name;

            if (assembly is not null && projectByAssembly.TryGetValue(assembly, out var project))
                typeId = KnowledgeId.ForType(TypeIdentity.Semantic(named), project).Value;
            else
                external = "true";
        }

        return ObservationPayload.From(
            ("parameter", parameter),
            ("ordinal", ordinal.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            ("form", form),
            ("value", value),
            ("typeId", typeId),
            ("external", external));
    }

    /// <summary>
    /// `notnull` e `unmanaged` são analisados como restrição de tipo, mas não
    /// são tipo: não aparecem em `ConstraintTypes`, e tratá-los como tipo
    /// desalinharia o caminhamento das duas listas. O símbolo é quem confirma.
    /// </summary>
    private static bool IsKeyword(TypeConstraintSyntax constraint, ITypeParameterSymbol? parameter)
    {
        if (parameter is null)
            return false;

        var written = constraint.Type.ToString();

        return (written == "notnull" && parameter.HasNotNullConstraint)
               || (written == "unmanaged" && parameter.HasUnmanagedTypeConstraint);
    }

    /// <summary>
    /// Tipo ou método parcial repete a cláusula em cada parte, e a linguagem
    /// exige que sejam idênticas. Sem esta guarda, o mesmo parâmetro
    /// receberia dois conjuntos de ordinais e IV-23 falharia.
    /// </summary>
    private static IEnumerable<TypeParameterConstraintClauseSyntax> Clauses(ISymbol symbol)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        var references = symbol.DeclaringSyntaxReferences
            .OrderBy(r => r.SyntaxTree.FilePath, StringComparer.Ordinal)
            .ThenBy(r => r.Span.Start);

        foreach (var reference in references)
        {
            var clauses = reference.GetSyntax() switch
            {
                TypeDeclarationSyntax type => type.ConstraintClauses,
                MethodDeclarationSyntax method => method.ConstraintClauses,
                DelegateDeclarationSyntax delegated => delegated.ConstraintClauses,
                _ => default
            };

            foreach (var clause in clauses)
            {
                if (seen.Add(clause.Name.Identifier.ValueText))
                    yield return clause;
            }
        }
    }

    private static string Normalize(string written)
        => string.Join(' ', written.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

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

    private static string? FileOf(SourceCompilation source, ISymbol symbol)
    {
        var path = symbol.Locations.FirstOrDefault(l => l.IsInSource)?.SourceTree?.FilePath;

        if (path is null)
            return null;

        return source.Files.FirstOrDefault(f => f.Tree.FilePath == path)?.RelativePath;
    }
}
