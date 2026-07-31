using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using X7.Knowledge.Model;

namespace X7.Knowledge.Acquisition.Roslyn;

/// <summary>
/// Obtém o código de cada projeto no melhor nível disponível.
/// </summary>
/// <remarks>
/// Tenta primeiro o caminho semântico, via MSBuildWorkspace: é o que sustenta
/// C04 em diante, onde herança e implementação precisam ser fatos resolvidos e
/// não deduções por nome. Se o SDK não estiver disponível ou a solução não
/// restaurar, cai para leitura sintática pura e **declara** a queda de nível.
/// Nunca degrada em silêncio (Constituição §5.3).
/// </remarks>
public sealed class CompilationProvider : IDisposable
{
    private MSBuildWorkspace? _workspace;

    public async ValueTask<IReadOnlyList<SourceCompilation>> AcquireAsync(
        SolutionFile solution,
        CancellationToken cancellationToken)
    {
        var semantic = await TryAcquireSemanticAsync(solution, cancellationToken);

        if (semantic is not null)
            return semantic;

        return solution.Projects
            .Select(p => AcquireSyntactic(solution, p.RelativePath, SemanticFailure))
            .ToArray();
    }

    private string SemanticFailure { get; set; } = "Modelo semântico não solicitado.";

    private async ValueTask<IReadOnlyList<SourceCompilation>?> TryAcquireSemanticAsync(
        SolutionFile solution,
        CancellationToken cancellationToken)
    {
        if (!MsBuildBootstrap.Ensure())
        {
            SemanticFailure =
                $"SDK do MSBuild indisponível: {MsBuildBootstrap.Failure}";

            return null;
        }

        var solutionPath = Path.Combine(solution.RootDirectory, solution.FileName);

        try
        {
            _workspace = MSBuildWorkspace.Create();
            _workspace.LoadMetadataForReferencedProjects = true;

            var loaded = await _workspace.OpenSolutionAsync(solutionPath, cancellationToken: cancellationToken);

            // O workspace não lança em falha de carga: acumula diagnósticos.
            // Lidos da propriedade, e não pelo evento WorkspaceFailed, que o
            // Roslyn 5.x marcou obsoleto.
            var failures = _workspace.Diagnostics
                .Select(d => PathNormalizer.Sanitize(d.Message, solution.RootDirectory))
                .ToArray();

            var loadedProjects = loaded.Projects.ToArray();

            if (loadedProjects.Length == 0)
            {
                SemanticFailure = failures.Length == 0
                    ? "Workspace abriu a solução sem carregar nenhum projeto."
                    : $"Workspace não carregou nenhum projeto. {Describe(failures)}";

                _workspace.Dispose();
                _workspace = null;

                return null;
            }

            var results = new List<SourceCompilation>();

            foreach (var entry in solution.Projects)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var project = loadedProjects.FirstOrDefault(p =>
                    p.FilePath is not null
                    && string.Equals(
                        PathNormalizer.ToRelative(solution.RootDirectory, p.FilePath),
                        entry.RelativePath,
                        StringComparison.OrdinalIgnoreCase));

                if (project is null)
                {
                    // Diagnóstico completo: sem os caminhos que o workspace de
                    // fato carregou, não dá para distinguir "não carregou nada"
                    // de "carregou com caminho diferente do esperado".
                    var loadedPaths = loadedProjects
                        .Where(p => p.FilePath is not null)
                        .Select(p => PathNormalizer.ToRelative(solution.RootDirectory, p.FilePath!))
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(p => p, StringComparer.Ordinal)
                        .Take(5)
                        .ToArray();

                    var detalhe = $"Esperado '{entry.RelativePath}'; "
                                  + $"workspace carregou {loadedProjects.Length} projeto(s)"
                                  + (loadedPaths.Length == 0
                                      ? ", nenhum com caminho conhecido."
                                      : $", por exemplo: {string.Join(", ", loadedPaths)}.");

                    results.Add(AcquireSyntactic(
                        solution,
                        entry.RelativePath,
                        failures.Length == 0
                            ? $"Projeto não encontrado no workspace. {detalhe}"
                            : $"Projeto não encontrado no workspace. {detalhe} {Describe(failures)}"));

                    continue;
                }

                var compilation = await project.GetCompilationAsync(cancellationToken);

                if (compilation is null)
                {
                    results.Add(AcquireSyntactic(
                        solution,
                        entry.RelativePath,
                        "Compilação semântica indisponível para o projeto."));

                    continue;
                }

                var files = compilation.SyntaxTrees
                    .Where(t => !string.IsNullOrEmpty(t.FilePath))
                    .Select(t => new SourceFile
                    {
                        RelativePath = PathNormalizer.ToRelative(solution.RootDirectory, t.FilePath),
                        Tree = t
                    })
                    .OrderBy(f => f.RelativePath, StringComparer.Ordinal)
                    .ToArray();

                results.Add(new SourceCompilation
                {
                    ProjectRelativePath = entry.RelativePath,
                    Level = AcquisitionLevel.Semantic,
                    Compilation = compilation,
                    Files = files,
                    Limitations = DiagnosticLimitations(compilation, entry.RelativePath)
                });
            }

            return results;
        }
        catch (Exception e) when (e is InvalidOperationException
                                      or IOException
                                      or NotSupportedException
                                      or ReflectionTypeLoadException)
        {
            SemanticFailure = "Workspace não pôde abrir a solução: "
                               + $"{e.GetType().Name} — {PathNormalizer.Sanitize(e.Message, solution.RootDirectory)}";

            _workspace?.Dispose();
            _workspace = null;

            return null;
        }
    }


    /// <summary>
    /// Resume os diagnósticos de carga sem despejar centenas de linhas na
    /// Base. Ordenado e limitado: a mensagem entra numa Observation, e
    /// Observation precisa ser determinística.
    /// </summary>
    private static string Describe(IReadOnlyCollection<string> failures)
    {
        const int Limit = 3;

        var distinct = failures
            .Distinct(StringComparer.Ordinal)
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToArray();

        var shown = string.Join(" | ", distinct.Take(Limit));

        return distinct.Length > Limit
            ? $"Diagnósticos ({distinct.Length}), primeiros {Limit}: {shown}"
            : $"Diagnósticos: {shown}";
    }
    /// <summary>
    /// Erro de compilação não impede leitura, mas compromete o modelo
    /// semântico. Declarado, nunca omitido.
    /// </summary>
    private static IReadOnlyList<AcquisitionLimitation> DiagnosticLimitations(
        Microsoft.CodeAnalysis.Compilation compilation,
        string projectRelativePath)
    {
        var errors = compilation
            .GetDiagnostics()
            .Count(d => d.Severity == DiagnosticSeverity.Error);

        if (errors == 0)
            return [];

        return
        [
            new AcquisitionLimitation
            {
                Reason = $"Projeto compila com {errors} erro(s); símbolos podem estar não resolvidos",
                AffectedScope = "semantic-model",
                Source = projectRelativePath
            }
        ];
    }

    private static SourceCompilation AcquireSyntactic(
        SolutionFile solution,
        string projectRelativePath,
        string reason)
    {
        var directory = Path.Combine(
            solution.RootDirectory,
            PathNormalizer.DirectoryOf(projectRelativePath).Replace('/', Path.DirectorySeparatorChar));

        var files = new List<SourceFile>();

        if (Directory.Exists(directory))
        {
            // D-01: ordenação canônica, nunca ordem de descoberta do disco.
            var paths = Directory
                .EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories)
                .Select(p => PathNormalizer.ToRelative(solution.RootDirectory, p))
                .Where(p => !p.Contains("/bin/", StringComparison.Ordinal)
                            && !p.Contains("/obj/", StringComparison.Ordinal))
                .OrderBy(p => p, StringComparer.Ordinal);

            foreach (var relative in paths)
            {
                var absolute = Path.Combine(
                    solution.RootDirectory,
                    relative.Replace('/', Path.DirectorySeparatorChar));

                files.Add(new SourceFile
                {
                    RelativePath = relative,
                    Tree = CSharpSyntaxTree.ParseText(
                        File.ReadAllText(absolute),
                        path: relative)
                });
            }
        }

        return new SourceCompilation
        {
            ProjectRelativePath = projectRelativePath,
            Level = AcquisitionLevel.Syntactic,
            Compilation = null,
            Files = files,
            Limitations =
            [
                new AcquisitionLimitation
                {
                    Reason = $"Modelo semântico indisponível: {reason}",
                    AffectedScope = "semantic-model",
                    Source = projectRelativePath
                }
            ]
        };
    }

    public void Dispose() => _workspace?.Dispose();
}
