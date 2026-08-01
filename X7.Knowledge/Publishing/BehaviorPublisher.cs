using System.Text;
using X7.Knowledge.Model;

namespace X7.Knowledge.Publishing;

/// <summary>Eixo de partição de <c>Behavior/</c>.</summary>
public enum BehaviorLayout
{
    /// <summary>Um arquivo por tipo. É a Base publicada (ADR-040).</summary>
    PerType,

    /// <summary>
    /// Um arquivo por projeto. Existe apenas para a medição comparativa que a
    /// ADR-040 §6 exige; nunca é a Base publicada.
    /// </summary>
    PerProject
}

/// <summary>
/// C05 — superfície pública, um arquivo por tipo (ADR-040).
/// </summary>
/// <remarks>
/// A unidade de consulta desta projeção é o tipo, não o projeto: quem
/// pergunta o que um tipo expõe já sabe qual é o tipo. Publicar por projeto
/// faria essa pergunta pagar a superfície inteira — estimado entre 4.000 e
/// 6.000 tokens contra os 712 do código-fonte equivalente, uma Base mais cara
/// que aquilo que ela substitui (AC-11).
///
/// O índice não lista nomes de tipo. O caminho é função da identidade, e o
/// nome do arquivo faz o trabalho do índice (BM-12).
/// </remarks>
public sealed class BehaviorPublisher : IPublisher
{
    /// <summary>Ordem canônica dos modificadores na assinatura escrita.</summary>
    private static readonly string[] ModifierOrder =
    [
        "static", "extern", "abstract", "virtual", "override", "sealed", "readonly", "required"
    ];

    private static readonly char[] InvalidInPath = ['<', '>', ':', '"', '/', '\\', '|', '?', '*'];

    private static readonly string[] Published =
    [
        TypeVocabulary.Public, TypeVocabulary.Protected, TypeVocabulary.ProtectedInternal
    ];

    private readonly BehaviorLayout _layout;

    public BehaviorPublisher(BehaviorLayout layout = BehaviorLayout.PerType)
        => _layout = layout;

    public async ValueTask PublishAsync(
        KnowledgeModel model,
        string outputDirectory,
        CancellationToken cancellationToken)
    {
        var facts = SurfaceFacts.Build(model);

        if (facts.IsEmpty)
            return;

        var counts = new List<(string Project, int Types)>();

        foreach (var project in model.Entities.Projects)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var types = facts.TypesOf(project.Id);

            if (types.Count == 0)
                continue;

            counts.Add((project.Name, types.Count));

            if (_layout == BehaviorLayout.PerProject)
            {
                var builder = new StringBuilder();

                builder.Append("# Superfície pública — ").Append(project.Name).Append("\n\n");

                foreach (var type in types)
                    facts.AppendType(builder, type, headingLevel: 2);

                await CanonicalFile.WriteAsync(
                    Path.Combine(outputDirectory, "Behavior", $"{project.Name}.md"),
                    builder.ToString(),
                    cancellationToken);

                continue;
            }

            foreach (var type in types)
            {
                var builder = new StringBuilder();

                facts.AppendType(builder, type, headingLevel: 1);

                await CanonicalFile.WriteAsync(
                    Path.Combine(
                        outputDirectory,
                        "Behavior",
                        project.Name,
                        facts.FileNameOf(type) + ".md"),
                    builder.ToString(),
                    cancellationToken);
            }
        }

        await CanonicalFile.WriteAsync(
            Path.Combine(outputDirectory, "Behavior", "INDEX.md"),
            BuildIndex(counts, _layout),
            cancellationToken);
    }

    private static string BuildIndex(
        IReadOnlyList<(string Project, int Types)> counts,
        BehaviorLayout layout)
    {
        var builder = new StringBuilder();

        builder.Append("# Índice de superfície pública\n\n");

        if (layout == BehaviorLayout.PerType)
        {
            builder.Append("Um arquivo por tipo, em `Behavior/{projeto}/`. ");
            builder.Append("O nome do arquivo é o nome qualificado do tipo, ");
            builder.Append("com `` ` `` de aridade genérica trocado por `-` e ");
            builder.Append("aninhamento por `+`. Nenhum dos dois é válido em ");
            builder.Append("identificador C#, então o nome nunca colide.\n\n");
            builder.Append("Exemplo: `Behavior/X7.Knowledge/X7.Knowledge.Model.KnowledgeModelBuilder.md`.\n\n");
        }
        else
        {
            builder.Append("Um arquivo por projeto. Layout de medição (ADR-040 §6), ");
            builder.Append("não é a Base publicada.\n\n");
        }

        builder.Append("Este índice não lista nomes de tipo: repetir conteúdo aqui ");
        builder.Append("anularia o ganho da partição.\n\n");

        builder.Append("| Projeto | Tipos publicados |\n|---|---|\n");

        foreach (var (project, types) in counts)
            builder.Append("| ").Append(project).Append(" | ").Append(types).Append(" |\n");

        return builder.ToString();
    }

    /// <summary>
    /// Índices de leitura sobre as Observations. Não calcula conhecimento:
    /// um Publisher que calculasse violaria PR-06. Compor a assinatura a
    /// partir de fatos decompostos é formatação, e é por isso que o modelo
    /// não guarda assinatura pronta (OB-01).
    /// </summary>
    private sealed class SurfaceFacts
    {
        private readonly Dictionary<KnowledgeId, TypeRecord> _types = [];
        private readonly Dictionary<KnowledgeId, MemberRecord> _members = [];
        private readonly Dictionary<KnowledgeId, List<KnowledgeId>> _byType = [];
        private readonly Dictionary<KnowledgeId, List<KnowledgeId>> _byProject = [];

        public bool IsEmpty => _members.Count == 0;

        public static SurfaceFacts Build(KnowledgeModel model)
        {
            var facts = new SurfaceFacts();

            foreach (var observation in model.Observations)
            {
                switch (observation.Kind)
                {
                    case ObservationKinds.TypeDeclared:
                        // Atualiza em vez de substituir: as Observations vêm
                        // ordenadas por subject e depois por kind, e
                        // `type.accessibility` chega antes de `type.declared`.
                        // Substituir o registro descartaria o que já foi lido.
                        var declared = facts.Type(observation.Subject);

                        declared.Name = observation.Payload["name"] ?? observation.Subject.Value;
                        declared.Namespace = observation.Payload["namespace"];
                        declared.ProjectId = KnowledgeId.Parse(observation.Payload["projectId"]!);

                        break;

                    case ObservationKinds.TypeKind:
                        facts.Type(observation.Subject).Kind = observation.Payload["kind"];
                        break;

                    case ObservationKinds.TypeAccessibility:
                        facts.Type(observation.Subject).Accessibility = observation.Payload["value"];
                        break;

                    case ObservationKinds.TypeModifier:
                        facts.Type(observation.Subject).Modifiers.Add(observation.Payload["name"]!);
                        break;

                    case ObservationKinds.TypeGenericParameter:
                        facts.Type(observation.Subject).Parameters.Add(
                            (Ordinal(observation), observation.Payload["name"]!));

                        break;

                    case ObservationKinds.TypeNestedIn:
                        facts.Type(observation.Subject).Container =
                            KnowledgeId.Parse(observation.Payload["containerId"]!);

                        break;

                    case ObservationKinds.TypeLocation:
                        facts.Type(observation.Subject).Files.Add(observation.Payload["file"]!);
                        break;

                    case ObservationKinds.TypeDeclaresMember:
                        Append(
                            facts._byType,
                            observation.Subject,
                            KnowledgeId.Parse(observation.Payload["memberId"]!));

                        break;

                    case ObservationKinds.MemberDeclared:
                        facts.Member(observation.Subject).Name = observation.Payload["name"];
                        facts.Member(observation.Subject).Kind = observation.Payload["kind"];
                        break;

                    case ObservationKinds.MemberAccessibility:
                        facts.Member(observation.Subject).Accessibility = observation.Payload["value"];
                        break;

                    case ObservationKinds.MemberModifier:
                        facts.Member(observation.Subject).Modifiers.Add(observation.Payload["name"]!);
                        break;

                    case ObservationKinds.MemberType:
                        facts.Member(observation.Subject).Type = observation.Payload["typeName"];
                        break;

                    case ObservationKinds.MemberParameter:
                        facts.Member(observation.Subject).Parameters.Add(new ParameterRecord
                        {
                            Ordinal = Ordinal(observation),
                            Name = observation.Payload["name"] ?? string.Empty,
                            Type = observation.Payload["typeName"] ?? string.Empty,
                            Modifier = observation.Payload["modifier"],
                            Optional = observation.Payload["optional"] is not null
                        });

                        break;

                    case ObservationKinds.MemberGenericParameter:
                        facts.Member(observation.Subject).TypeParameters.Add(
                            (Ordinal(observation), observation.Payload["name"]!));

                        break;

                    case ObservationKinds.MemberAccessor:
                        facts.Member(observation.Subject).Accessors.Add(
                            (observation.Payload["kind"]!, observation.Payload["accessibility"]));

                        break;
                }
            }

            foreach (var (typeId, members) in facts._byType)
            {
                if (!facts._types.TryGetValue(typeId, out var type))
                    continue;

                if (!members.Any(m => facts.IsPublished(m)))
                    continue;

                Append(facts._byProject, type.ProjectId, typeId);
            }

            return facts;
        }

        public IReadOnlyList<KnowledgeId> TypesOf(KnowledgeId projectId)
        {
            if (!_byProject.TryGetValue(projectId, out var types))
                return Array.Empty<KnowledgeId>();

            return types.OrderBy(FileNameOf, StringComparer.Ordinal).ToArray();
        }

        /// <summary>
        /// Nome de arquivo derivado da identidade do tipo (ADR-040): nome
        /// qualificado, aridade genérica com `-`, aninhamento com `+`.
        /// </summary>
        public string FileNameOf(KnowledgeId typeId)
        {
            var type = Type(typeId);

            var parts = new List<string> { type.Name };

            var container = type.Container;

            var guard = 0;

            while (container is { } id && _types.ContainsKey(id) && guard++ < 32)
            {
                parts.Insert(0, Type(id).Name);
                container = Type(id).Container;
            }

            var qualified = string.Join('+', parts);

            var full = string.IsNullOrEmpty(type.Namespace)
                ? qualified
                : type.Namespace + "." + qualified;

            var name = full.Replace('`', '-');

            // Falha alta em vez de caminho torto: um nome inválido aqui
            // significa que algo entrou na Base que não deveria ter entrado,
            // e escrever assim mesmo esconderia a causa (ADR-041).
            if (name.IndexOfAny(InvalidInPath) >= 0)
            {
                throw new InvalidOperationException(
                    $"Nome de tipo inválido em caminho de arquivo: '{name}'. "
                    + "Verifique a fronteira de observação (ADR-041).");
            }

            return name;
        }

        public void AppendType(StringBuilder builder, KnowledgeId typeId, int headingLevel)
        {
            var type = Type(typeId);

            var heading = new string('#', headingLevel);

            builder.Append(heading).Append(' ').Append(Declaration(type)).Append("\n\n");

            if (!string.IsNullOrEmpty(type.Namespace))
                builder.Append("Namespace `").Append(type.Namespace).Append("`. ");

            if (type.Files.Count > 0)
            {
                builder.Append("Declarado em `")
                       .Append(string.Join("`, `", type.Files.OrderBy(f => f, StringComparer.Ordinal)))
                       .Append("`.");
            }

            builder.Append("\n\n");

            KnowledgeId[] members = _byType.TryGetValue(typeId, out var all)
                ? all.Where(IsPublished).ToArray()
                : Array.Empty<KnowledgeId>();

            AppendSection(builder, headingLevel + 1, "Construtores", members, MemberVocabulary.Constructor, type);
            AppendSection(builder, headingLevel + 1, "Propriedades", members, MemberVocabulary.Property, type);
            AppendSection(builder, headingLevel + 1, "Métodos", members, MemberVocabulary.Method, type);
        }

        private void AppendSection(
            StringBuilder builder,
            int headingLevel,
            string title,
            IReadOnlyList<KnowledgeId> members,
            string kind,
            TypeRecord type)
        {
            var selected = members
                .Where(m => string.Equals(Member(m).Kind, kind, StringComparison.Ordinal))
                .OrderBy(m => Signature(Member(m), type), StringComparer.Ordinal)
                .ToArray();

            if (selected.Length == 0)
                return;

            builder.Append(new string('#', headingLevel)).Append(' ').Append(title).Append("\n\n");

            foreach (var member in selected)
                builder.Append("- `").Append(Signature(Member(member), type)).Append("`\n");

            builder.Append('\n');
        }

        /// <summary>Declaração do próprio tipo, como está escrita (ADR-036).</summary>
        private string Declaration(TypeRecord type)
        {
            var parts = new List<string>();

            if (type.Accessibility is { } accessibility)
                parts.Add(accessibility.Replace('-', ' '));

            parts.AddRange(Ordered(type.Modifiers));

            if (type.Kind is { } kind)
                parts.Add(kind.Replace('-', ' '));

            parts.Add(Short(NameWithParameters(type)));

            return string.Join(' ', parts);
        }

        private static string NameWithParameters(TypeRecord type)
        {
            var name = Strip(type.Name);

            if (type.Parameters.Count == 0)
                return name;

            var ordered = type.Parameters.OrderBy(p => p.Ordinal).Select(p => p.Name);

            return $"{name}<{string.Join(", ", ordered)}>";
        }

        private string Signature(MemberRecord member, TypeRecord type)
        {
            var parts = new List<string>();

            if (member.Accessibility is { } accessibility)
                parts.Add(accessibility.Replace('-', ' '));

            parts.AddRange(Ordered(member.Modifiers));

            if (member.Type is { } declared)
                parts.Add(Short(declared));

            var name = string.Equals(member.Kind, MemberVocabulary.Constructor, StringComparison.Ordinal)
                ? Strip(type.Name)
                : Strip(member.Name ?? string.Empty);

            if (member.TypeParameters.Count > 0)
            {
                var ordered = member.TypeParameters.OrderBy(p => p.Ordinal).Select(p => p.Name);

                name += $"<{string.Join(", ", ordered)}>";
            }

            if (string.Equals(member.Kind, MemberVocabulary.Property, StringComparison.Ordinal))
            {
                parts.Add(name + " { " + Accessors(member) + " }");

                return string.Join(' ', parts);
            }

            parts.Add(name + "(" + Parameters(member) + ")");

            return string.Join(' ', parts);
        }

        private string Accessors(MemberRecord member)
        {
            var ordered = member.Accessors
                .OrderBy(a => a.Kind, StringComparer.Ordinal)
                .Select(a => a.Accessibility is null
                    ? a.Kind + ";"
                    : a.Accessibility.Replace('-', ' ') + " " + a.Kind + ";");

            return string.Join(' ', ordered);
        }

        private string Parameters(MemberRecord member)
        {
            var ordered = member.Parameters
                .OrderBy(p => p.Ordinal)
                .Select(p =>
                {
                    var text = p.Modifier is null
                        ? string.Empty
                        : p.Modifier.Replace('-', ' ') + " ";

                    text += Short(p.Type) + " " + p.Name;

                    return p.Optional ? text + " = …" : text;
                });

            return string.Join(", ", ordered);
        }

        private static IEnumerable<string> Ordered(IEnumerable<string> modifiers)
        {
            var present = modifiers.ToHashSet(StringComparer.Ordinal);

            return ModifierOrder.Where(m => present.Contains(m));
        }

        private bool IsPublished(KnowledgeId memberId)
            => _members.TryGetValue(memberId, out var member)
               && member.Accessibility is { } accessibility
               && Published.Contains(accessibility, StringComparer.Ordinal);

        private TypeRecord Type(KnowledgeId id)
        {
            if (!_types.TryGetValue(id, out var record))
            {
                record = new TypeRecord { Name = id.Value, Namespace = null, ProjectId = id };
                _types[id] = record;
            }

            return record;
        }

        private MemberRecord Member(KnowledgeId id)
        {
            if (!_members.TryGetValue(id, out var record))
            {
                record = new MemberRecord();
                _members[id] = record;
            }

            return record;
        }

        private static void Append(
            Dictionary<KnowledgeId, List<KnowledgeId>> target,
            KnowledgeId key,
            KnowledgeId value)
        {
            if (!target.TryGetValue(key, out var values))
            {
                values = [];
                target[key] = values;
            }

            if (!values.Contains(value))
                values.Add(value);
        }

        private static int Ordinal(Observation observation)
            => int.TryParse(
                observation.Payload["ordinal"],
                System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture,
                out var ordinal)
                ? ordinal
                : 0;

        /// <summary>Aridade de metadados não é escrita em lugar nenhum do C#.</summary>
        private static string Strip(string name)
        {
            var arity = name.IndexOf('`', StringComparison.Ordinal);

            return arity < 0 ? name : name[..arity];
        }

        /// <summary>
        /// Nome de tipo sem namespace, para leitura. O modelo guarda o nome
        /// qualificado; encurtar é formatação, e é o mesmo que
        /// `Relations/` já faz.
        /// </summary>
        private static string Short(string qualified)
        {
            var builder = new StringBuilder();

            var segment = new StringBuilder();

            foreach (var c in qualified)
            {
                if (char.IsLetterOrDigit(c) || c is '_' or '.')
                {
                    segment.Append(c);
                    continue;
                }

                builder.Append(LastSegment(segment.ToString()));
                segment.Clear();
                builder.Append(c);
            }

            builder.Append(LastSegment(segment.ToString()));

            return builder.ToString();
        }

        private static string LastSegment(string value)
        {
            var dot = value.LastIndexOf('.');

            return dot < 0 ? value : value[(dot + 1)..];
        }

        private sealed class TypeRecord
        {
            public required string Name { get; set; }

            public string? Namespace { get; set; }

            public required KnowledgeId ProjectId { get; set; }

            public string? Kind { get; set; }

            public string? Accessibility { get; set; }

            public KnowledgeId? Container { get; set; }

            public SortedSet<string> Modifiers { get; } = new(StringComparer.Ordinal);

            public List<(int Ordinal, string Name)> Parameters { get; } = [];

            public SortedSet<string> Files { get; } = new(StringComparer.Ordinal);
        }

        private sealed class MemberRecord
        {
            public string? Name { get; set; }

            public string? Kind { get; set; }

            public string? Accessibility { get; set; }

            public string? Type { get; set; }

            public SortedSet<string> Modifiers { get; } = new(StringComparer.Ordinal);

            public List<ParameterRecord> Parameters { get; } = [];

            public List<(int Ordinal, string Name)> TypeParameters { get; } = [];

            public List<(string Kind, string? Accessibility)> Accessors { get; } = [];
        }

        private sealed record ParameterRecord
        {
            public required int Ordinal { get; init; }

            public required string Name { get; init; }

            public required string Type { get; init; }

            public required string? Modifier { get; init; }

            public required bool Optional { get; init; }
        }
    }
}
