using X7.Knowledge;
using X7.Knowledge.Publishing;
using Xunit;

namespace X7.KnowledgeTests;

/// <summary>
/// Critério de conclusão 3 do C01 e IV-06.
/// Este é o teste que separa "funciona" de "é um compilador".
/// </summary>
public sealed class DeterminismTests : IClassFixture<SolutionFixture>
{
    private readonly SolutionFixture _fixture;

    public DeterminismTests(SolutionFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Duas_compilacoes_produzem_saida_byte_identica()
    {
        var first = await CompileToTempAsync();
        var second = await CompileToTempAsync();

        var left = Snapshot(first);
        var right = Snapshot(second);

        Assert.Equal(left.Keys.OrderBy(k => k, StringComparer.Ordinal),
                     right.Keys.OrderBy(k => k, StringComparer.Ordinal));

        foreach (var (path, bytes) in left)
            Assert.True(bytes.SequenceEqual(right[path]), $"Arquivo divergente: {path}");
    }

    [Fact]
    public async Task Saida_json_nao_contem_BOM_nem_CRLF()
    {
        var output = await CompileToTempAsync();

        var path = Path.Combine(output, "model", "knowledge.model.json");

        var bytes = await File.ReadAllBytesAsync(path);

        Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);
        Assert.DoesNotContain((byte)'\r', bytes);
    }

    [Fact]
    public async Task Saida_nao_contem_caminho_absoluto_da_maquina()
    {
        var output = await CompileToTempAsync();

        foreach (var file in Directory.EnumerateFiles(output, "*", SearchOption.AllDirectories))
        {
            var content = await File.ReadAllTextAsync(file);

            Assert.DoesNotContain(_fixture.Root, content, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task Digest_das_entradas_muda_quando_o_conteudo_muda()
    {
        var before = await KnowledgeCompiler.CompileAsync(
            _fixture.SolutionPath, NewOutput());

        var csproj = Path.Combine(_fixture.Root, "tools", "Cli", "Cli.csproj");
        var original = await File.ReadAllTextAsync(csproj);

        try
        {
            await File.WriteAllTextAsync(csproj, original.Replace("Exe", "Library"));

            var after = await KnowledgeCompiler.CompileAsync(
                _fixture.SolutionPath, NewOutput());

            Assert.NotEqual(before.Manifest.InputDigest, after.Manifest.InputDigest);
        }
        finally
        {
            await File.WriteAllTextAsync(csproj, original);
        }
    }

    private async Task<string> CompileToTempAsync()
    {
        var output = NewOutput();

        await KnowledgeCompiler.CompileAsync(_fixture.SolutionPath, output);

        return output;
    }

    private string NewOutput()
        => Path.Combine(_fixture.Root, "out-" + Guid.NewGuid().ToString("n"));

    private static Dictionary<string, byte[]> Snapshot(string directory)
        => Directory
            .EnumerateFiles(directory, "*", SearchOption.AllDirectories)
            .ToDictionary(
                f => Path.GetRelativePath(directory, f).Replace('\\', '/'),
                File.ReadAllBytes,
                StringComparer.Ordinal);
}
