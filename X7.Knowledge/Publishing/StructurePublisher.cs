using System.Globalization;
using System.Text;
using X7.Knowledge.Model;

namespace X7.Knowledge.Publishing;

/// <summary>
/// C03 — publica um arquivo por projeto mais um índice enxuto.
/// </summary>
/// <remarks>
/// Projeção monolítica seria o pior caso para a métrica: qualquer pergunta
/// sobre um tipo pagaria a solução inteira. Um arquivo por projeto acompanha
/// a granularidade com que as perguntas são feitas e mantém cada arquivo em
/// tamanho comparável ao de `Structure/Solution.md`.
/// </remarks>
public sealed class StructurePublisher : IPublisher
{
    public async ValueTask PublishAsync(
        KnowledgeModel model,
        string outputDirectory,
        CancellationToken cancellationToken)
    {
        var types = model.Observations
            .Where(o => o.Kind == ObservationKinds.TypeDeclared)
            .ToArray();

        if (types.Length == 0)
            return;

        var byProject = types
            .GroupBy(o => KnowledgeId.Parse(o.Payload["projectId"]!))
            .ToDictionary(g => g.Key, g => g.ToArray());

        // Varrer o modelo inteiro por tipo publicado é quadrático e a Base
        // cresce com a solução. Os índices são montados uma vez.
        var facts = TypeFacts.Build(model);

        foreach (var project in model.Entities.Projects)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!byProject.TryGetValue(project.Id, out var projectTypes))
                continue;

            await CanonicalFile.WriteAsync(
                Path.Combine(outputDirectory, "Structure", "Types", $"{project.Name}.md"),
                BuildProject(facts, project.Name, projectTypes),
                cancellationToken);
        }

        await CanonicalFile.WriteAsync(
            Path.Combine(outputDirectory, "Structure", "Types", "INDEX.md"),
            BuildIndex(model, byProject),
            cancellationToken);

        await CanonicalFile.WriteAsync(
            Path.Combine(outputDirectory, "Structure", "Namespaces.md"),
            BuildNamespaces(model),
            cancellationToken);
    }

    /// <summary>
    /// Índices de leitura sobre as Observations de tipo. Não produz nada:
    /// um Publisher que calculasse conhecimento violaria PR-06.
    /// </summary>
    private sealed class TypeFacts
    {
        private readonly Dictionary<KnowledgeId, List<string>> _locations = [];
        private readonly Dictionary<KnowledgeId, List<string>> _modifiers = [];
        private readonly Dictionary<KnowledgeId, List<(int Ordinal, string Name)>> _parameters = [];
        private readonly Dictionary<KnowledgeId, string> _kinds = [];
        private readonly Dictionary<KnowledgeId, string> _accessibilities = [];

        public static TypeFacts Build(KnowledgeModel model)
        {
            var facts = new TypeFacts();

            foreach (var observation in model.Observations)
            {
                switch (observation.Kind)
                {
                    case ObservationKinds.TypeLocation:
                        Append(facts._locations, observation.Subject, observation.Payload["file"]!);
                        break;

                    case ObservationKinds.TypeModifier:
                        Append(facts._modifiers, observation.Subject, observation.Payload["name"]!);
                        break;

                    case ObservationKinds.TypeKind:
                        facts._kinds[observation.Subject] = observation.Payload["kind"]!;
                        break;

                    case ObservationKinds.TypeAccessibility:
                        facts._accessibilities[observation.Subject] = observation.Payload["value"]!;
                        break;

                    case ObservationKinds.TypeGenericParameter:
                        if (!facts._parameters.TryGetValue(observation.Subject, out var parameters))
                        {
                            parameters = [];
                            facts._parameters[observation.Subject] = parameters;
                        }

                        parameters.Add((
                            int.Parse(observation.Payload["ordinal"]!, CultureInfo.InvariantCulture),
                            observation.Payload["name"]!));

                        break;
                }
            }

            return facts;
        }

        private static void Append(
            Dictionary<KnowledgeId, List<string>> target,
            KnowledgeId subject,
            string value)
        {
            if (!target.TryGetValue(subject, out var values))
            {
                values = [];
                target[subject] = values;
            }

            values.Add(value);
        }

        public string Kind(KnowledgeId typeId) => _kinds.GetValueOrDefault(typeId, "—");

        /// <summary>
        /// Todos os arquivos de declaração, ordenados. Tipo parcial tem mais
        /// de um; escolher só o primeiro mandaria quem procura ao lugar
        /// errado metade das vezes.
        /// </summary>
        public string Location(KnowledgeId typeId)
        {
            if (!_locations.TryGetValue(typeId, out var files))
                return "—";

            return string.Join("`, `", files.OrderBy(f => f, StringComparer.Ordinal));
        }

        /// <summary>Acessibilidade e modificadores, na ordem em que se escreve.</summary>
        public string Declaration(KnowledgeId typeId)
        {
            var parts = new List<string>();

            if (_accessibilities.TryGetValue(typeId, out var accessibility))
                parts.Add(accessibility.Replace('-', ' '));

            if (_modifiers.TryGetValue(typeId, out var modifiers))
                parts.AddRange(modifiers.OrderBy(m => m, StringComparer.Ordinal));

            return parts.Count == 0 ? "—" : string.Join(' ', parts);
        }

        /// <summary>
        /// Nome como foi declarado, a partir do nome curto de metadados, que
        /// vem com crase e aridade e não é o que ninguém escreve nem procura.
        /// Os parâmetros genéricos já estão na Base; reconstruir
        /// `Cache&lt;TKey, TValue&gt;` a partir deles não inventa nada.
        /// </summary>
        /// <remarks>
        /// Recebe o nome curto (`name`), nunca o qualificado
        /// (`metadataName`): em nível S o qualificado já traz
        /// `&lt;TKey, TValue&gt;` e a lista sairia duplicada, além de repetir
        /// o namespace que já é coluna da tabela.
        /// </remarks>
        public string Display(KnowledgeId typeId, string shortName)
        {
            var arity = shortName.IndexOf('`', StringComparison.Ordinal);

            var name = arity < 0 ? shortName : shortName[..arity];

            if (!_parameters.TryGetValue(typeId, out var parameters) || parameters.Count == 0)
                return name;

            var ordered = parameters.OrderBy(p => p.Ordinal).Select(p => p.Name);

            return $"{name}<{string.Join(", ", ordered)}>";
        }
    }

    private static string BuildProject(
        TypeFacts facts,
        string projectName,
        IReadOnlyList<Observation> types)
    {
        var builder = new StringBuilder();

        builder.Append("# Tipos — ").Append(projectName).Append("\n\n");
        builder.Append(types.Count).Append(" tipo(s).\n\n");

        // Seccionado por classificação (ADR-035). Classificação é consultada
        // junto com o inventário — "onde está X e o que X é" é uma pergunta
        // só. Relação de tipo não é, e fica em Relations/ (§9.1).
        foreach (var group in types
                     .GroupBy(o => facts.Kind(o.Subject))
                     .OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            builder.Append("## ").Append(group.Key).Append("\n\n");
            builder.Append("| Tipo | Namespace | Declaração | Arquivo |\n|---|---|---|---|\n");

            foreach (var type in group.OrderBy(o => o.Payload["metadataName"], StringComparer.Ordinal))
            {
                builder.Append("| ")
                       .Append(facts.Display(type.Subject, type.Payload["name"]!))
                       .Append(" | ").Append(type.Payload["namespace"] ?? "(global)")
                       .Append(" | ").Append(facts.Declaration(type.Subject))
                       .Append(" | `").Append(facts.Location(type.Subject))
                       .Append("` |\n");
            }

            builder.Append('\n');
        }

        return builder.ToString();
    }

    private static string BuildIndex(
        KnowledgeModel model,
        IReadOnlyDictionary<KnowledgeId, Observation[]> byProject)
    {
        var builder = new StringBuilder();

        builder.Append("# Índice de tipos\n\n");
        builder.Append("Um arquivo por projeto. Abra apenas o que precisar.\n\n");
        builder.Append("| Projeto | Tipos | Arquivo |\n|---|---|---|\n");

        foreach (var project in model.Entities.Projects)
        {
            if (!byProject.TryGetValue(project.Id, out var types))
                continue;

            builder.Append("| ").Append(project.Name)
                   .Append(" | ").Append(types.Length)
                   .Append(" | [`").Append(project.Name).Append(".md`](")
                   .Append(project.Name).Append(".md) |\n");
        }

        return builder.ToString();
    }

    private static string BuildNamespaces(KnowledgeModel model)
    {
        var builder = new StringBuilder();

        builder.Append("# Namespaces\n\n");

        var declared = model.Observations
            .Where(o => o.Kind == ObservationKinds.NamespaceDeclared)
            .OrderBy(o => o.Payload["name"], StringComparer.Ordinal)
            .ToArray();

        var counts = model.Observations
            .Where(o => o.Kind == ObservationKinds.NamespaceContains)
            .GroupBy(o => o.Subject)
            .ToDictionary(g => g.Key, g => g.Count());

        builder.Append("| Namespace | Tipos diretos |\n|---|---|\n");

        foreach (var observation in declared)
        {
            builder.Append("| ").Append(observation.Payload["name"])
                   .Append(" | ").Append(counts.GetValueOrDefault(observation.Subject, 0))
                   .Append(" |\n");
        }

        return builder.ToString();
    }
}
