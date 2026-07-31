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
                ProjectReferences = [],
                PackageReferences = [],
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

        var packageElements = root
            .Elements(ns + "ItemGroup")
            .SelectMany(g => g.Elements(ns + "PackageReference"))
            .Where(e => !string.IsNullOrWhiteSpace(e.Attribute("Include")?.Value))
            .ToArray();

        var packageReferences = packageElements
            .Select(e => e.Attribute("Include")!.Value)
            .ToArray();

        var packages = packageElements
            .Select(e => new PackageReference
            {
                Name = e.Attribute("Include")!.Value.Trim(),
                Version = Unresolved(e.Attribute("Version")?.Value)
                    ? null
                    : e.Attribute("Version")?.Value.Trim()
            })
            .DistinctBy(x => x.Name, StringComparer.Ordinal)
            .OrderBy(x => x.Name, StringComparer.Ordinal)
            .ToArray();

        foreach (var package in packageElements
                     .Where(e => Unresolved(e.Attribute("Version")?.Value))
                     .OrderBy(e => e.Attribute("Include")!.Value, StringComparer.Ordinal))
        {
            limitations.Add(new AcquisitionLimitation
            {
                Reason = $"Versão não resolvida do pacote '{package.Attribute("Include")!.Value}'",
                AffectedScope = "project-package-reference",
                Source = relativePath,
                Locator = package.Attribute("Include")!.Value
            });
        }

        var projectDirectory = PathNormalizer.DirectoryOf(relativePath);

        var projectReferences = new List<string>();

        foreach (var reference in root
                     .Elements(ns + "ItemGroup")
                     .SelectMany(g => g.Elements(ns + "ProjectReference"))
                     .Select(e => e.Attribute("Include")?.Value)
                     .Where(v => !string.IsNullOrWhiteSpace(v))
                     .Select(v => v!.Trim()))
        {
            if (reference.Contains("$(", StringComparison.Ordinal))
            {
                limitations.Add(new AcquisitionLimitation
                {
                    Reason = $"Referência de projeto não resolvida: '{reference}'",
                    AffectedScope = "project-reference",
                    Source = relativePath,
                    Locator = reference
                });

                continue;
            }

            projectReferences.Add(Combine(projectDirectory, reference));
        }

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
            ProjectReferences = projectReferences
                .Distinct(StringComparer.Ordinal)
                .OrderBy(r => r, StringComparer.Ordinal)
                .ToArray(),
            PackageReferences = packages,
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

    private static bool Unresolved(string? value)
        => value is not null && value.Contains("$(", StringComparison.Ordinal);

    /// <summary>
    /// Resolve um caminho relativo ao diretório do .csproj, produzindo
    /// caminho relativo à raiz da solução com separador '/' (D-02).
    /// </summary>
    private static string Combine(string projectDirectory, string reference)
    {
        var segments = new List<string>();

        if (projectDirectory != ".")
            segments.AddRange(projectDirectory.Split('/', StringSplitOptions.RemoveEmptyEntries));

        foreach (var segment in reference.Replace('\\', '/')
                                         .Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            switch (segment)
            {
                case ".":
                    continue;

                case "..":
                    if (segments.Count > 0)
                        segments.RemoveAt(segments.Count - 1);
                    continue;

                default:
                    segments.Add(segment);
                    continue;
            }
        }

        return string.Join('/', segments);
    }
}
