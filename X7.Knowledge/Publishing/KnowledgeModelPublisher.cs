using X7.Knowledge.Model;
using X7.Knowledge.Serialization;

namespace X7.Knowledge.Publishing;

/// <summary>Publica a forma canônica em Knowledge/model/knowledge.model.json.</summary>
public sealed class KnowledgeModelPublisher : IPublisher
{
    public async ValueTask PublishAsync(
        KnowledgeModel model,
        string outputDirectory,
        CancellationToken cancellationToken)
    {
        var json = Serialize(model).Serialize();

        var path = Path.Combine(outputDirectory, "model", "knowledge.model.json");

        await CanonicalFile.WriteAsync(path, json, cancellationToken);
    }

    internal static CanonicalJson Serialize(KnowledgeModel model)
        => CanonicalJson.Object(
            ("manifest", SerializeManifest(model.Manifest)),
            ("observations", CanonicalJson.Array(
                model.Observations.Select(SerializeObservation))),
            ("entities", SerializeEntities(model)));

    private static CanonicalJson SerializeManifest(Manifest manifest)
        => CanonicalJson.Object(
            ("modelVersion", CanonicalJson.Of(manifest.ModelVersion)),
            ("compilerVersion", CanonicalJson.Of(manifest.CompilerVersion)),
            ("solutionId", CanonicalJson.Of(manifest.SolutionId.Value)),
            ("acquisitionLevel", CanonicalJson.Of(manifest.AcquisitionLevel.ToToken())),
            ("capabilities", CanonicalJson.Strings(manifest.Capabilities)),
            ("inputDigest", CanonicalJson.Of(manifest.InputDigest)),
            ("observationCount", CanonicalJson.Of(manifest.ObservationCount)));

    private static CanonicalJson SerializeObservation(Observation observation)
        => CanonicalJson.Object(
            ("id", CanonicalJson.Of(observation.Id.Value)),
            ("kind", CanonicalJson.Of(observation.Kind)),
            ("subject", CanonicalJson.Of(observation.Subject.Value)),
            ("payload", CanonicalJson.Object(
                observation.Payload.Values
                    .Select(p => (p.Key, (CanonicalJson?)CanonicalJson.Of(p.Value)))
                    .ToArray())),
            ("provenance", CanonicalJson.Object(
                ("source", CanonicalJson.Of(observation.Provenance.Source)),
                ("locator", observation.Provenance.Locator is null
                    ? null
                    : CanonicalJson.Of(observation.Provenance.Locator)),
                ("producer", CanonicalJson.Of(observation.Provenance.Producer)),
                ("capability", CanonicalJson.Of(observation.Provenance.Capability)),
                ("acquisitionLevel", CanonicalJson.Of(
                    observation.Provenance.AcquisitionLevel.ToToken())))));

    private static CanonicalJson SerializeEntities(KnowledgeModel model)
    {
        var solution = model.Entities.Solution;

        return CanonicalJson.Object(
            ("solution", CanonicalJson.Object(
                ("id", CanonicalJson.Of(solution.Id.Value)),
                ("name", CanonicalJson.Of(solution.Name)),
                ("projects", CanonicalJson.Strings(solution.Projects.Select(p => p.Value))),
                ("folders", CanonicalJson.Strings(solution.Folders.Select(f => f.Value))))),
            ("projects", CanonicalJson.Array(model.Entities.Projects.Select(p =>
                CanonicalJson.Object(
                    ("id", CanonicalJson.Of(p.Id.Value)),
                    ("name", CanonicalJson.Of(p.Name)),
                    ("relativePath", CanonicalJson.Of(p.RelativePath)),
                    ("directory", CanonicalJson.Of(p.Directory)),
                    ("targetFrameworks", CanonicalJson.Strings(p.TargetFrameworks)),
                    ("outputKind", p.OutputKind is null ? null : CanonicalJson.Of(p.OutputKind)),
                    ("languageVersion", p.LanguageVersion is null ? null : CanonicalJson.Of(p.LanguageVersion)),
                    ("isTestProject", p.IsTestProject is null ? null : CanonicalJson.Of(p.IsTestProject.Value)))))),
            ("folders", CanonicalJson.Array(model.Entities.Folders.Select(f =>
                CanonicalJson.Object(
                    ("id", CanonicalJson.Of(f.Id.Value)),
                    ("name", CanonicalJson.Of(f.Name)),
                    ("parent", f.Parent is null ? null : CanonicalJson.Of(f.Parent.Value.Value)),
                    ("children", CanonicalJson.Strings(f.Children.Select(c => c.Value))))))));
    }
}
