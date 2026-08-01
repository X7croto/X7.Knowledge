using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using X7.Knowledge.Acquisition.Roslyn;
using X7.Knowledge.Model;

namespace X7.Knowledge.Compilation.Producers;

/// <summary>
/// C05, primeira fatia — superfície declarada de métodos, construtores e
/// propriedades (KNOWLEDGE_MODEL §6.1.4, ADR-039).
/// </summary>
/// <remarks>
/// Exige nível S e não tem caminho sintático paralelo: assinatura resolvida
/// por sintaxe seria dedução por nome, que §5.3 proíbe. Em nível X o Producer
/// declara a limitação e não produz nada.
///
/// Todos os membros declarados são observados, de qualquer acessibilidade. A
/// projeção é que filtra a superfície pública. Observar só o público tornaria
/// a ausência ambígua — *não existe* e *existe e é privado* ficariam
/// indistinguíveis — e ampliar depois o alcance de um kind existente é
/// alterá-lo, que EX-01 proíbe.
/// </remarks>
public sealed class MemberSurfaceProducer : IProducer
{
    private readonly IReadOnlyList<SourceCompilation> _sources;

    public MemberSurfaceProducer(IReadOnlyList<SourceCompilation> sources)
        => _sources = sources;

    public string Name => nameof(MemberSurfaceProducer);

    public string Capability => "C05";

    public ValueTask ProduceAsync(
        CompilationContext context,
        CancellationToken cancellationToken)
    {
        // Nome do assembly para nome do projeto: o símbolo conhece o
        // assembly, e a identidade é ancorada no projeto.
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
                DeclareLevelLimitation(context, source);

                continue;
            }

            Produce(context, source, projectByAssembly);
        }

        DeclareSliceLimitation(context);

        return ValueTask.CompletedTask;
    }

    private void DeclareLevelLimitation(CompilationContext context, SourceCompilation source)
        => context.Knowledge.Add(
            ObservationKinds.AcquisitionLimitation,
            KnowledgeId.ForProject(source.ProjectRelativePath),
            ObservationPayload.From(
                ("reason",
                    "Membros e assinaturas exigem nível S; projeto lido apenas sintaticamente"),
                ("affectedScope", "type-members")),
            new Provenance
            {
                Source = source.ProjectRelativePath,
                Producer = Name,
                Capability = Capability,
                AcquisitionLevel = AcquisitionLevel.Syntactic
            });

    /// <summary>
    /// A fatia cobre método, construtor e propriedade. O que falta é ausência
    /// declarada, nunca silenciosa (§6.1).
    /// </summary>
    private void DeclareSliceLimitation(CompilationContext context)
        => context.Knowledge.Add(
            ObservationKinds.AcquisitionLimitation,
            context.SolutionId,
            ObservationPayload.From(
                ("reason",
                    "Campos, eventos, operadores, indexadores, construtores estáticos, "
                    + "implementações explícitas de interface e restrições genéricas "
                    + "ainda não são observados"),
                ("affectedScope", "type-members-partial")),
            new Provenance
            {
                Source = context.Solution.FileName,
                Producer = Name,
                Capability = Capability,
                AcquisitionLevel = context.AcquisitionLevel
            });

    private void Produce(
        CompilationContext context,
        SourceCompilation source,
        IReadOnlyDictionary<string, string> projectByAssembly)
    {
        var projectName = projectByAssembly[source.Compilation!.Assembly.Name];

        foreach (var type in SourceTypes(source.Compilation!.Assembly.GlobalNamespace)
                     .OrderBy(TypeIdentity.Semantic, StringComparer.Ordinal))
        {
            var typeId = KnowledgeId.ForType(TypeIdentity.Semantic(type), projectName);

            foreach (var member in Surface(type).OrderBy(MemberIdentity.Semantic, StringComparer.Ordinal))
            {
                var file = FileOf(source, member);

                if (file is null)
                    continue;

                var memberId = KnowledgeId.ForMember(MemberIdentity.Semantic(member), projectName);

                var provenance = new Provenance
                {
                    Source = file,
                    Producer = Name,
                    Capability = Capability,
                    AcquisitionLevel = AcquisitionLevel.Semantic
                };

                Emit(context, typeId, memberId, member, provenance, projectByAssembly);
            }
        }
    }

    /// <summary>
    /// O que a declaração escreve e esta fatia cobre.
    /// </summary>
    /// <remarks>
    /// Membro implícito não entra: construtor padrão gerado, `Equals`,
    /// `GetHashCode`, `ToString`, `Deconstruct` e `&lt;Clone&gt;$` de record,
    /// e os métodos `get_X`/`set_X` da propriedade — estes últimos
    /// representados por `member.accessor`. É o argumento das bases
    /// implícitas do C04: observar o que a linguagem gera produziria
    /// Observations por tipo sem informar nada.
    ///
    /// Construtor estático, operador e indexador ficam para a fatia seguinte,
    /// com limitação declarada.
    /// </remarks>
    private static IEnumerable<ISymbol> Surface(INamedTypeSymbol type)
        => type.GetMembers().Where(Included);

    private static bool Included(ISymbol member)
    {
        if (member.IsImplicitlyDeclared)
            return false;

        return member switch
        {
            IMethodSymbol method
                => method.MethodKind is MethodKind.Ordinary or MethodKind.Constructor,
            IPropertySymbol property => !property.IsIndexer,
            _ => false
        };
    }

    private void Emit(
        CompilationContext context,
        KnowledgeId typeId,
        KnowledgeId memberId,
        ISymbol member,
        Provenance provenance,
        IReadOnlyDictionary<string, string> projectByAssembly)
    {
        var kind = KindOf(member);

        if (!MemberVocabulary.IsKnownKind(kind))
            throw new InvalidOperationException($"Espécie de membro fora do vocabulário: '{kind}'.");

        var accessibility = AccessibilityOf(member.DeclaredAccessibility);

        if (!TypeVocabulary.IsKnownAccessibility(accessibility))
            throw new InvalidOperationException($"Acessibilidade fora do vocabulário: '{accessibility}'.");

        context.Knowledge.Add(
            ObservationKinds.TypeDeclaresMember,
            typeId,
            ObservationPayload.From(("memberId", memberId.Value)),
            provenance);

        context.Knowledge.Add(
            ObservationKinds.MemberDeclared,
            memberId,
            ObservationPayload.From(("name", member.Name), ("kind", kind)),
            provenance);

        context.Knowledge.Add(
            ObservationKinds.MemberAccessibility,
            memberId,
            ObservationPayload.From(("value", accessibility)),
            provenance);

        foreach (var modifier in DeclaredModifiers(member))
        {
            context.Knowledge.Add(
                ObservationKinds.MemberModifier,
                memberId,
                ObservationPayload.From(("name", modifier)),
                provenance);
        }

        EmitType(context, memberId, member, provenance, projectByAssembly);

        if (member is IMethodSymbol method)
        {
            EmitParameters(context, memberId, method, provenance, projectByAssembly);
            EmitGenericParameters(context, memberId, method, provenance);
        }

        if (member is IPropertySymbol property)
            EmitAccessors(context, memberId, property, provenance);
    }

    /// <summary>
    /// O tipo escrito na posição de tipo da declaração: retorno do método,
    /// tipo da propriedade. Construtor não recebe nenhum — a declaração não
    /// escreve tipo ali, e escrever `void` seria fabricar.
    /// </summary>
    private void EmitType(
        CompilationContext context,
        KnowledgeId memberId,
        ISymbol member,
        Provenance provenance,
        IReadOnlyDictionary<string, string> projectByAssembly)
    {
        var declared = member switch
        {
            IMethodSymbol { MethodKind: MethodKind.Ordinary } method => method.ReturnType,
            IPropertySymbol property => property.Type,
            _ => null
        };

        if (declared is null)
            return;

        context.Knowledge.Add(
            ObservationKinds.MemberType,
            memberId,
            Reference(declared, projectByAssembly),
            provenance);
    }

    private void EmitParameters(
        CompilationContext context,
        KnowledgeId memberId,
        IMethodSymbol method,
        Provenance provenance,
        IReadOnlyDictionary<string, string> projectByAssembly)
    {
        foreach (var parameter in method.Parameters.OrderBy(p => p.Ordinal))
        {
            var modifier = ParameterModifierOf(parameter);

            if (modifier is not null && !MemberVocabulary.IsKnownParameterModifier(modifier))
                throw new InvalidOperationException($"Modificador de parâmetro fora do vocabulário: '{modifier}'.");

            var reference = Reference(parameter.Type, projectByAssembly);

            context.Knowledge.Add(
                ObservationKinds.MemberParameter,
                memberId,
                ObservationPayload.From(
                    ("name", parameter.Name),
                    ("ordinal", parameter.Ordinal.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                    ("typeName", reference["typeName"]),
                    ("typeId", reference["typeId"]),
                    ("external", reference["external"]),
                    ("modifier", modifier),
                    ("optional", parameter.HasExplicitDefaultValue ? "true" : null)),
                provenance);
        }
    }

    /// <summary>
    /// O ordinal está no payload pelo mesmo motivo do C04: D-01 ordena a
    /// saída por identidade canônica, e a ordem de declaração é semântica.
    /// Parâmetro de tipo de método não admite variância — só interface e
    /// delegate admitem —, então o payload não tem `variance`.
    /// </summary>
    private void EmitGenericParameters(
        CompilationContext context,
        KnowledgeId memberId,
        IMethodSymbol method,
        Provenance provenance)
    {
        foreach (var parameter in method.TypeParameters.OrderBy(p => p.Ordinal))
        {
            context.Knowledge.Add(
                ObservationKinds.MemberGenericParameter,
                memberId,
                ObservationPayload.From(
                    ("name", parameter.Name),
                    ("ordinal", parameter.Ordinal.ToString(System.Globalization.CultureInfo.InvariantCulture))),
                provenance);
        }
    }

    /// <summary>
    /// Acessibilidade do acessor só é registrada quando difere da
    /// propriedade: repetir a igual encheria a Base de fato que já está
    /// escrito uma linha acima.
    /// </summary>
    private void EmitAccessors(
        CompilationContext context,
        KnowledgeId memberId,
        IPropertySymbol property,
        Provenance provenance)
    {
        if (property.GetMethod is { } getter)
            EmitAccessor(context, memberId, MemberVocabulary.Get, getter, property, provenance);

        if (property.SetMethod is { } setter)
        {
            EmitAccessor(
                context,
                memberId,
                setter.IsInitOnly ? MemberVocabulary.Init : MemberVocabulary.Set,
                setter,
                property,
                provenance);
        }
    }

    private void EmitAccessor(
        CompilationContext context,
        KnowledgeId memberId,
        string kind,
        IMethodSymbol accessor,
        IPropertySymbol property,
        Provenance provenance)
    {
        if (!MemberVocabulary.IsKnownAccessor(kind))
            throw new InvalidOperationException($"Acessor fora do vocabulário: '{kind}'.");

        var accessibility = accessor.DeclaredAccessibility == property.DeclaredAccessibility
            ? null
            : AccessibilityOf(accessor.DeclaredAccessibility);

        context.Knowledge.Add(
            ObservationKinds.MemberAccessor,
            memberId,
            ObservationPayload.From(("kind", kind), ("accessibility", accessibility)),
            provenance);
    }

    /// <summary>
    /// Tipo de fora da solução não vira identidade do modelo — não existe lá.
    /// Parâmetro de tipo (`T`) e arranjo também não: são formas, não tipos
    /// declarados. Em todos esses casos fica o nome, que é conhecimento
    /// legítimo, e a marca `external`.
    /// </summary>
    private static ObservationPayload Reference(
        ITypeSymbol type,
        IReadOnlyDictionary<string, string> projectByAssembly)
    {
        var display = TypeIdentity.Display(type);

        if (type is INamedTypeSymbol named)
        {
            var assembly = named.OriginalDefinition.ContainingAssembly?.Name;

            if (assembly is not null && projectByAssembly.TryGetValue(assembly, out var project))
            {
                return ObservationPayload.From(
                    ("typeName", display),
                    ("typeId", KnowledgeId.ForType(TypeIdentity.Semantic(named), project).Value));
            }
        }

        return ObservationPayload.From(("typeName", display), ("external", "true"));
    }

    private static string KindOf(ISymbol member) => member switch
    {
        IMethodSymbol { MethodKind: MethodKind.Constructor } => MemberVocabulary.Constructor,
        IPropertySymbol => MemberVocabulary.Property,
        _ => MemberVocabulary.Method
    };

    /// <summary>
    /// Padrão da linguagem para membro é `private`, e o símbolo já resolve a
    /// omissão. `NotApplicable` não ocorre em membro de origem; se ocorrer,
    /// o padrão é o correto.
    /// </summary>
    private static string AccessibilityOf(Accessibility accessibility) => accessibility switch
    {
        Accessibility.Public => TypeVocabulary.Public,
        Accessibility.Internal => TypeVocabulary.Internal,
        Accessibility.Protected => TypeVocabulary.Protected,
        Accessibility.ProtectedOrInternal => TypeVocabulary.ProtectedInternal,
        Accessibility.ProtectedAndInternal => TypeVocabulary.PrivateProtected,
        _ => TypeVocabulary.Private
    };

    /// <summary>
    /// Modificadores vêm da declaração, e não do símbolo, pelo motivo já
    /// registrado no C04: o símbolo expõe a forma de metadados. Todo membro
    /// de interface é `IsAbstract` e `IsVirtual`, e publicar isso encheria a
    /// Base de modificador que ninguém escreveu.
    /// </summary>
    private static IEnumerable<string> DeclaredModifiers(ISymbol member)
    {
        var modifiers = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var reference in member.DeclaringSyntaxReferences)
        {
            foreach (var token in RawModifiers(reference.GetSyntax()))
            {
                if (MemberVocabulary.IsKnownModifier(token.ValueText))
                    modifiers.Add(token.ValueText);
            }
        }

        return modifiers;
    }

    private static SyntaxTokenList RawModifiers(SyntaxNode node) => node switch
    {
        MethodDeclarationSyntax method => method.Modifiers,
        ConstructorDeclarationSyntax constructor => constructor.Modifiers,
        BasePropertyDeclarationSyntax property => property.Modifiers,
        _ => default
    };

    private static string? ParameterModifierOf(IParameterSymbol parameter)
    {
        if (parameter.IsParams)
            return "params";

        return parameter.RefKind switch
        {
            RefKind.Ref => "ref",
            RefKind.Out => "out",
            RefKind.In => "in",
            _ => null
        };
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

    /// <summary>
    /// Caminho relativo do arquivo que declara o membro. Nulo quando o
    /// arquivo não pertence a este projeto — devolver o caminho absoluto
    /// como último recurso violaria IV-08.
    /// </summary>
    private static string? FileOf(SourceCompilation source, ISymbol symbol)
    {
        var path = symbol.Locations.FirstOrDefault(l => l.IsInSource)?.SourceTree?.FilePath;

        if (path is null)
            return null;

        return source.Files.FirstOrDefault(f => f.Tree.FilePath == path)?.RelativePath;
    }
}
