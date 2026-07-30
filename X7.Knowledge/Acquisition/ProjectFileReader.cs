using System.Xml.Linq;

namespace X7.Knowledge.Acquisition;

/// <summary>
/// Lê o .csproj como XML, sem MSBuild. Consequência declarada: imports,
/// Directory.Build.props e propriedades com $(...) não são resolvidos —
/// cada caso vira uma acquisition.limitation, nunca ausência silenciosa.
/// </summary>
public static class ProjectFileReader
{
    private static readonly string[] ScalarProperties =
    [
        "AssemblyName",
        "RootNamespace",
        "Nullable",
        "ImplicitUsings",
        "IsPackable",
        "IsTestProject",
        "GenerateDocumentationFile",
        "TreatWarningsAsErrors"
    ];

    private static readonly string[] TestPackageMarkers =
    [
        "Microsoft.NET.Test.Sdk",
        "xunit",
        "xunit.v3",
        "NUnit",
        "MSTest",
        "MSTest.TestFramework",
        "TUnit"
    ];

    public static ProjectFile Read(string rootDirectory, string relativePath)
    {
        var absolute = Path.Combine(rootDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));

        var limitations = new List<AcquisitionLimitation>();

        if (!File.Exists(absolute))
        {
            return new ProjectFile
            {
                RelativePath = relativePath,
                TargetFrameworks = [],
                Properties = new Dictionary<string, string>(StringComparer.Ordinal),
                Limitations =
                [
                    new AcquisitionLimitation
                    {
                        Reason = "Arquivo de projeto declarado na solução não existe no disco",
                        AffectedScope = "project",
                        Source = relativePath
                    }
                ]
            };
        }

        var document = XDocument.Load(absolute, LoadOptions.None);
        var root = document.Root!;

        // Sdk-style sem namespace; projetos antigos usam o namespace MSBuild.
        var ns = root.Name.Namespace;

        var propertyGroups = root.Elements(ns + "PropertyGroup").ToArray();

        var properties = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var element in propertyGroups.SelectMany(g => g.Elements()))
        {
            var name = element.Name.LocalName;
            var value = element.Value.Trim();

            if (value.Length == 0)
                continue;

            if (value.Contains("$(", StringComparison.Ordinal))
            {
                limitations.Add(new AcquisitionLimitation
                {
                    Reason = $"Propriedade '{name}' contém referência não resolvida: '{value}'",
                    AffectedScope = "project-property",
                    Source = relativePath,
                    Locator = name
                });

                continue;
            }

            // Condition em PropertyGroup torna o valor dependente de contexto de build.
            var condition = element.Parent?.Attribute("Condition")?.Value
                            ?? element.Attribute("Condition")?.Value;

            if (condition is not null)
            {
                limitations.Add(new AcquisitionLimitation
                {
                    Reason = $"Propriedade '{name}' é condicional: '{condition}'",
                    AffectedScope = "project-property",
                    Source = relativePath,
                    Locator = name
                });

                continue;
            }

            properties[name] = value;
        }

        foreach (var import in root.Elements(ns + "Import"))
        {
            limitations.Add(new AcquisitionLimitation
            {
                Reason = $"Import não resolvido: '{import.Attribute("Project")?.Value ?? "?"}'",
                AffectedScope = "project-property",
                Source = relativePath,
                Locator = "Import"
            });
        }

        DetectDirectoryBuildProps(rootDirectory, relativePath, limitations);

        var frameworks = ReadFrameworks(properties);

        if (frameworks.Count == 0)
        {
            limitations.Add(new AcquisitionLimitation
            {
                Reason = "Nenhum TargetFramework declarado diretamente no arquivo",
                AffectedScope = "project-target-framework",
                Source = relativePath
            });
        }

        var packageReferences = root
            .Elements(ns + "ItemGroup")
            .SelectMany(g => g.Elements(ns + "PackageReference"))
            .Select(p => p.Attribute("Include")?.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!)
            .ToArray();

        var (isTest, evidence) = DetectTestProject(properties, packageReferences);

        return new ProjectFile
        {
            RelativePath = relativePath,
            TargetFrameworks = frameworks,
            OutputKind = properties.GetValueOrDefault("OutputType"),
            LanguageVersion = properties.GetValueOrDefault("LangVersion"),
            Properties = properties
                .Where(p => ScalarProperties.Contains(p.Key, StringComparer.Ordinal))
                .ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal),
            IsTestProject = isTest,
            TestEvidence = evidence,
            Limitations = limitations
        };
    }

    private static IReadOnlyList<string> ReadFrameworks(
        IReadOnlyDictionary<string, string> properties)
    {
        if (properties.TryGetValue("TargetFrameworks", out var multiple))
        {
            return multiple
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(f => f, StringComparer.Ordinal)
                .ToArray();
        }

        if (properties.TryGetValue("TargetFramework", out var single))
            return [single];

        return [];
    }

    private static (bool IsTest, string? Evidence) DetectTestProject(
        IReadOnlyDictionary<string, string> properties,
        IReadOnlyList<string> packageReferences)
    {
        if (properties.TryGetValue("IsTestProject", out var declared)
            && bool.TryParse(declared, out var parsed)
            && parsed)
        {
            return (true, "property:IsTestProject");
        }

        var marker = packageReferences
            .Where(p => TestPackageMarkers.Contains(p, StringComparer.OrdinalIgnoreCase))
            .OrderBy(p => p, StringComparer.Ordinal)
            .FirstOrDefault();

        return marker is not null
            ? (true, $"package:{marker}")
            : (false, null);
    }

    private static void DetectDirectoryBuildProps(
        string rootDirectory,
        string relativePath,
        List<AcquisitionLimitation> limitations)
    {
        var directory = PathNormalizer.DirectoryOf(relativePath);

        while (true)
        {
            var candidate = directory == "."
                ? Path.Combine(rootDirectory, "Directory.Build.props")
                : Path.Combine(
                    rootDirectory,
                    directory.Replace('/', Path.DirectorySeparatorChar),
                    "Directory.Build.props");

            if (File.Exists(candidate))
            {
                limitations.Add(new AcquisitionLimitation
                {
                    Reason = "Directory.Build.props presente e não resolvido (leitura sintática)",
                    AffectedScope = "project-property",
                    Source = PathNormalizer.ToRelative(rootDirectory, candidate)
                });

                return;
            }

            if (directory == ".")
                return;

            directory = PathNormalizer.DirectoryOf(directory);
        }
    }
}
