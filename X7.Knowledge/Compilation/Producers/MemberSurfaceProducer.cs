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
    /// e os acessores que a propriedade e o evento em forma de campo geram —
    /// representados, quando declarados, por `member.accessor`. É o argumento
    /// das bases implícitas do C04: observar o que a linguagem gera produziria
    /// Observations por tipo sem informar nada.
    /// </remarks>
    private static IEnumerable<ISymbol> Surface(INamedTypeSymbol type)
        => type.GetMembers().Where(Included);

    private static bool Included(ISymbol member)
    {
        if (member.IsImplicitlyDeclared)
            return false;

        return member switch
        {
            IMethodSymbol method => method.MethodKind
                is MethodKind.Ordinary
                or MethodKind.Constructor
                or MethodKind.StaticConstructor
                or MethodKind.UserDefinedOperator
                or MethodKind.Conversion
                or MethodKind.ExplicitInterfaceImplementation,
            IPropertySymbol => true,
            IFieldSymbol => true,
            IEventSymbol => true,
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
            ObservationPayload.From(("name", NameOf(member)), ("kind", kind)),
            provenance);

        context.Knowledge.Add(
            ObservationKinds.MemberAccessibility,
            memberId,
            ObservationPayload.From(("value", accessibility)),
            provenance);

        var modifiers = DeclaredModifiers(member).ToArray();

        foreach (var modifier in modifiers)
        {
            context.Knowledge.Add(
                ObservationKinds.MemberModifier,
                memberId,
                ObservationPayload.From(("name", modifier)),
                provenance);
        }

        // O valor de uma constante pública é contrato, e não dado: ele é
        // embutido no chamador em tempo de compilação, e trocá-lo quebra quem
        // já compilou sem recompilação e sem aviso (ADR-044).
        //
        // A condição é o modificador **escrito**, e não `IFieldSymbol.IsConst`:
        // membro de enum também é constante para o símbolo, mas a declaração
        // dele não escreve `const` e a projeção não tem o que completar ali.
        if (member is IFieldSymbol && modifiers.Contains("const"))
        {
            var value = ConstantValueOf(member);

            if (value is not null)
            {
                context.Knowledge.Add(
                    ObservationKinds.MemberConstantValue,
                    memberId,
                    ObservationPayload.From(("value", value)),
                    provenance);
            }
        }

        EmitType(context, memberId, member, provenance, projectByAssembly);
        EmitExplicitInterfaces(context, memberId, member, provenance, projectByAssembly);

        if (member is IMethodSymbol method)
        {
            EmitParameters(context, memberId, method.Parameters, provenance, projectByAssembly);
            EmitGenericParameters(context, memberId, method, provenance);
        }

        if (member is IPropertySymbol { IsIndexer: true } indexer)
            EmitParameters(context, memberId, indexer.Parameters, provenance, projectByAssembly);

        if (member is IPropertySymbol property)
            EmitAccessors(context, memberId, property, provenance);

        if (member is IEventSymbol declaredEvent)
            EmitEventAccessors(context, memberId, declaredEvent, provenance);
    }

    /// <summary>
    /// O que a declaração escreve como nome. Para operador é o próprio
    /// símbolo — `+`, `implicit`, `explicit` —, e não `op_Addition`, que é
    /// forma de metadados e fica reservada à identidade. Para indexador é
    /// `this`. Mesma divisão que a fatia A fez entre `MetadataName` e `Name`.
    /// </summary>
    private static string NameOf(ISymbol member)
    {
        if (member is IPropertySymbol { IsIndexer: true })
            return "this";

        foreach (var reference in member.DeclaringSyntaxReferences)
        {
            switch (reference.GetSyntax())
            {
                case OperatorDeclarationSyntax declared:
                    return declared.OperatorToken.ValueText;

                case ConversionOperatorDeclarationSyntax conversion:
                    return conversion.ImplicitOrExplicitKeyword.ValueText;
            }
        }

        return member.Name;
    }

    /// <summary>
    /// Implementação explícita continua sendo método, propriedade ou evento
    /// (ADR-042). A acessibilidade dela é `private`, que é o que o símbolo
    /// responde e o que está nos metadados — C# proíbe modificador de acesso
    /// ali, então não há declaração para espelhar, e publicar `public` seria
    /// fabricar. Quem decide que ela é superfície é a projeção, pela presença
    /// deste fato, e não pela acessibilidade.
    /// </summary>
    private void EmitExplicitInterfaces(
        CompilationContext context,
        KnowledgeId memberId,
        ISymbol member,
        Provenance provenance,
        IReadOnlyDictionary<string, string> projectByAssembly)
    {
        var implemented = member switch
        {
            IMethodSymbol method => method.ExplicitInterfaceImplementations.Cast<ISymbol>(),
            IPropertySymbol property => property.ExplicitInterfaceImplementations.Cast<ISymbol>(),
            IEventSymbol declared => declared.ExplicitInterfaceImplementations.Cast<ISymbol>(),
            _ => Enumerable.Empty<ISymbol>()
        };

        foreach (var target in implemented
                     .Select(s => s.ContainingType)
                     .Where(t => t is not null)
                     .OrderBy(TypeIdentity.Display, StringComparer.Ordinal))
        {
            context.Knowledge.Add(
                ObservationKinds.MemberExplicitInterface,
                memberId,
                Reference(target, projectByAssembly, "interfaceName", "interfaceId"),
                provenance);
        }
    }

    /// <summary>
    /// Evento em forma de campo não declara acessor nenhum: os que o símbolo
    /// expõe são gerados, e observá-los seria observar o que a linguagem
    /// escreveu no lugar de quem programou.
    /// </summary>
    private void EmitEventAccessors(
        CompilationContext context,
        KnowledgeId memberId,
        IEventSymbol declared,
        Provenance provenance)
    {
        if (declared.AddMethod is { IsImplicitlyDeclared: false })
            EmitAccessor(context, memberId, MemberVocabulary.Add, null, provenance);

        if (declared.RemoveMethod is { IsImplicitlyDeclared: false })
            EmitAccessor(context, memberId, MemberVocabulary.Remove, null, provenance);
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
            IMethodSymbol
            {
                MethodKind: MethodKind.Constructor or MethodKind.StaticConstructor
            } => null,
            IMethodSymbol method => method.ReturnType,
            IPropertySymbol property => property.Type,
            IFieldSymbol field => field.Type,
            IEventSymbol declaredEvent => declaredEvent.Type,
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
        IEnumerable<IParameterSymbol> parameters,
        Provenance provenance,
        IReadOnlyDictionary<string, string> projectByAssembly)
    {
        foreach (var parameter in parameters.OrderBy(p => p.Ordinal))
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
                    ("optional", parameter.HasExplicitDefaultValue ? "true" : null),
                    ("defaultValue", DefaultValueOf(parameter))),
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
        {
            EmitAccessor(
                context,
                memberId,
                MemberVocabulary.Get,
                Differing(getter, property),
                provenance);
        }

        if (property.SetMethod is { } setter)
        {
            EmitAccessor(
                context,
                memberId,
                setter.IsInitOnly ? MemberVocabulary.Init : MemberVocabulary.Set,
                Differing(setter, property),
                provenance);
        }
    }

    private static string? Differing(IMethodSymbol accessor, ISymbol owner)
        => accessor.DeclaredAccessibility == owner.DeclaredAccessibility
            ? null
            : AccessibilityOf(accessor.DeclaredAccessibility);

    private void EmitAccessor(
        CompilationContext context,
        KnowledgeId memberId,
        string kind,
        string? accessibility,
        Provenance provenance)
    {
        if (!MemberVocabulary.IsKnownAccessor(kind))
            throw new InvalidOperationException($"Acessor fora do vocabulário: '{kind}'.");

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
        IReadOnlyDictionary<string, string> projectByAssembly,
        string nameKey = "typeName",
        string idKey = "typeId")
    {
        var display = TypeIdentity.Display(type);

        if (type is INamedTypeSymbol named)
        {
            var assembly = named.OriginalDefinition.ContainingAssembly?.Name;

            if (assembly is not null && projectByAssembly.TryGetValue(assembly, out var project))
            {
                return ObservationPayload.From(
                    (nameKey, display),
                    (idKey, KnowledgeId.ForType(TypeIdentity.Semantic(named), project).Value));
            }
        }

        return ObservationPayload.From((nameKey, display), ("external", "true"));
    }

    private static string KindOf(ISymbol member) => member switch
    {
        IMethodSymbol
        {
            MethodKind: MethodKind.Constructor or MethodKind.StaticConstructor
        } => MemberVocabulary.Constructor,

        IMethodSymbol
        {
            MethodKind: MethodKind.UserDefinedOperator or MethodKind.Conversion
        } => MemberVocabulary.Operator,

        IPropertySymbol { IsIndexer: true } => MemberVocabulary.Indexer,
        IPropertySymbol => MemberVocabulary.Property,
        IFieldSymbol => MemberVocabulary.Field,
        IEventSymbol => MemberVocabulary.Event,
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
        OperatorDeclarationSyntax declared => declared.Modifiers,
        ConversionOperatorDeclarationSyntax conversion => conversion.Modifiers,

        // Cobre propriedade, indexador e evento com acessores.
        BasePropertyDeclarationSyntax property => property.Modifiers,

        // Campo e evento em forma de campo declaram pelo declarador; os
        // modificadores estão dois níveis acima, na declaração que pode
        // conter vários nomes.
        VariableDeclaratorSyntax declarator
            when declarator.Parent?.Parent is BaseFieldDeclarationSyntax field => field.Modifiers,

        _ => default
    };

    /// <summary>
    /// Da sintaxe, e não de `RefKind` (ADR-043). O mapeamento anterior partia
    /// do enum e `ref readonly` caía no ramo padrão: o vocabulário declarava
    /// `ref-readonly` desde a fatia A e nada podia produzi-lo. Ausência
    /// silenciosa, que dois conjuntos de testes não pegaram porque a fixture
    /// não tinha o caso.
    /// </summary>
    private static string? ParameterModifierOf(IParameterSymbol parameter)
    {
        var written = WrittenModifiers(parameter);

        if (written.Contains("params"))
            return "params";

        if (written.Contains("ref") && written.Contains("readonly"))
            return "ref-readonly";

        if (written.Contains("ref"))
            return "ref";

        if (written.Contains("out"))
            return "out";

        if (written.Contains("in"))
            return "in";

        return null;
    }

    private static HashSet<string> WrittenModifiers(IParameterSymbol parameter)
    {
        var written = new HashSet<string>(StringComparer.Ordinal);

        foreach (var reference in parameter.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not ParameterSyntax syntax)
                continue;

            foreach (var token in syntax.Modifiers)
                written.Add(token.ValueText);
        }

        return written;
    }

    /// <summary>
    /// Como está escrito, pelo mesmo motivo do valor padrão de parâmetro:
    /// `"x"`, `1`, `Kind.None` e `default` são formas distintas que os
    /// metadados achatam.
    /// </summary>
    private static string? ConstantValueOf(ISymbol member)
    {
        foreach (var reference in member.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is VariableDeclaratorSyntax { Initializer: { } initializer })
                return initializer.Value.ToString();
        }

        return null;
    }

    /// <summary>
    /// Como está escrito, e não reconstruído de `ExplicitDefaultValue`:
    /// `default` e `null` são a mesma coisa nos metadados e coisas
    /// diferentes na declaração.
    /// </summary>
    private static string? DefaultValueOf(IParameterSymbol parameter)
    {
        if (!parameter.HasExplicitDefaultValue)
            return null;

        foreach (var reference in parameter.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is ParameterSyntax { Default: { } clause })
                return clause.Value.ToString();
        }

        return null;
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
