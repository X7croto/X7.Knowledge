using System.Security.Cryptography;
using System.Text;
using X7.Knowledge.Acquisition;

namespace X7.Knowledge.Compilation;

/// <summary>
/// D-07: hash canônico das entradas consideradas.
/// Depende apenas de caminho relativo e conteúdo — nunca de timestamp ou ordem de disco.
/// </summary>
internal static class InputDigest
{
    public static string Compute(SolutionFile solution)
    {
        var inputs = new List<string> { solution.FileName };

        inputs.AddRange(solution.Projects.Select(p => p.RelativePath));

        var builder = new StringBuilder();

        foreach (var relative in inputs.OrderBy(i => i, StringComparer.Ordinal))
        {
            var absolute = Path.Combine(
                solution.RootDirectory,
                relative.Replace('/', Path.DirectorySeparatorChar));

            var contentHash = File.Exists(absolute)
                ? Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(absolute))).ToLowerInvariant()
                : "absent";

            builder.Append(relative).Append('\u0000').Append(contentHash).Append('\n');
        }

        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()));

        return Convert.ToHexString(digest).ToLowerInvariant();
    }
}
