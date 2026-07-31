using X7.Knowledge;
using Xunit;

namespace X7.KnowledgeTests;

/// <summary>
/// A Base anterior não pode ser perdida por falha de publicação.
/// Regenerar é barato; perder um resultado versionado não é.
/// </summary>
public sealed class PublishSafetyTests : IClassFixture<SolutionFixture>
{
    private readonly SolutionFixture _fixture;

    public PublishSafetyTests(SolutionFixture fixture) => _fixture = fixture;

    private string NewOutput()
        => Path.Combine(_fixture.Root, "out-" + Guid.NewGuid().ToString("n"));

    [Fact]
    public async Task Recompilar_substitui_a_base_sem_deixar_residuo()
    {
        var output = NewOutput();

        await KnowledgeCompiler.CompileAsync(_fixture.SolutionPath, output);
        await KnowledgeCompiler.CompileAsync(_fixture.SolutionPath, output);

        Assert.True(File.Exists(Path.Combine(output, "model", "knowledge.model.json")));
        Assert.True(File.Exists(Path.Combine(output, "README.md")));
        Assert.True(File.Exists(Path.Combine(output, "Structure", "Solution.md")));

        Assert.False(Directory.Exists(output + ".staging"));
        Assert.False(Directory.Exists(output + ".previous"));
    }

    [Fact]
    public async Task Diretorio_que_nao_e_base_publicada_nao_e_substituido()
    {
        var output = NewOutput();

        Directory.CreateDirectory(output);

        var importante = Path.Combine(output, "codigo-do-usuario.cs");

        await File.WriteAllTextAsync(importante, "// não apague isso");

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await KnowledgeCompiler.CompileAsync(_fixture.SolutionPath, output));

        Assert.True(File.Exists(importante));
        Assert.Equal("// não apague isso", await File.ReadAllTextAsync(importante));
    }

    [Fact]
    public async Task Base_anterior_sobrevive_quando_a_publicacao_falha()
    {
        var output = NewOutput();

        await KnowledgeCompiler.CompileAsync(_fixture.SolutionPath, output);

        var modelPath = Path.Combine(output, "model", "knowledge.model.json");
        var original = await File.ReadAllBytesAsync(modelPath);

        // Ocupa o nome da área de preparo com um arquivo: Directory.CreateDirectory falha.
        var staging = output + ".staging";

        await File.WriteAllTextAsync(staging, "bloqueado");

        try
        {
            await Assert.ThrowsAnyAsync<Exception>(async () =>
                await KnowledgeCompiler.CompileAsync(_fixture.SolutionPath, output));

            Assert.True(File.Exists(modelPath), "Base anterior foi perdida.");
            Assert.Equal(original, await File.ReadAllBytesAsync(modelPath));
        }
        finally
        {
            if (File.Exists(staging))
                File.Delete(staging);
        }
    }

    [Fact]
    public async Task Descarte_travado_nao_impede_publicacao()
    {
        var output = NewOutput();

        await KnowledgeCompiler.CompileAsync(_fixture.SolutionPath, output);

        // Ocupa o nome padrão de descarte com um arquivo: não pode ser apagado
        // nem sobrescrito por Directory.Move.
        var blocked = output + ".previous";

        await File.WriteAllTextAsync(blocked, "travado");

        try
        {
            var model = await KnowledgeCompiler.CompileAsync(_fixture.SolutionPath, output);

            Assert.True(File.Exists(Path.Combine(output, "model", "knowledge.model.json")));
            Assert.True(model.Manifest.ObservationCount > 0);
        }
        finally
        {
            if (File.Exists(blocked))
                File.Delete(blocked);

            foreach (var leftover in Directory.EnumerateDirectories(
                         Path.GetDirectoryName(output)!,
                         Path.GetFileName(output) + ".previous*"))
            {
                Directory.Delete(leftover, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Primeira_compilacao_em_diretorio_inexistente_funciona()
    {
        var output = Path.Combine(NewOutput(), "aninhado", "Knowledge");

        await KnowledgeCompiler.CompileAsync(_fixture.SolutionPath, output);

        Assert.True(File.Exists(Path.Combine(output, "model", "knowledge.model.json")));
    }
}
