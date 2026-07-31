using X7.Knowledge;
using X7.Knowledge.Compilation;
using X7.Knowledge.Acquisition;
using X7.Knowledge.Model;
using Xunit;

namespace X7.KnowledgeTests;

/// <summary>C03 — namespaces, tipos e localização.</summary>
public sealed class CodeStructureTests : IClassFixture<SolutionFixture>
{
    private readonly SolutionFixture _fixture;

    public CodeStructureTests(SolutionFixture fixture) => _fixture = fixture;

    private Task<KnowledgeModel> CompileAsync()
        => KnowledgeCompiler.CompileAsync(
            _fixture.SolutionPath,
            Path.Combine(_fixture.Root, "out-" + Guid.NewGuid().ToString("n"))).AsTask();

    private static IReadOnlyList<Observation> Types(KnowledgeModel model)
        => model.Observations.Where(o => o.Kind == ObservationKinds.TypeDeclared).ToArray();

    [Fact]
    public async Task Invariantes_continuam_passando_com_C03()
    {
        var model = await CompileAsync();

        Assert.Empty(ModelInvariants.Validate(model));
        Assert.Contains("C03", model.Manifest.Capabilities);
    }

    [Fact]
    public async Task Tipos_declarados_sao_observados()
    {
        var model = await CompileAsync();

        var names = Types(model)
            .Select(o => o.Payload["name"])
            .ToArray();

        Assert.Contains("Order", names);
        Assert.Contains("IOrderPolicy", names);
        Assert.Contains("Money", names);
        Assert.Contains("Clock", names);
    }

    [Fact]
    public async Task Tipo_aninhado_e_observado_com_nome_qualificado()
    {
        var model = await CompileAsync();

        Assert.Contains(Types(model), o =>
            o.Payload["name"] == "Line"
            && o.Payload["metadataName"]!.Contains("Order", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Namespace_e_hierarquia_sao_observados()
    {
        var model = await CompileAsync();

        var namespaces = model.Observations
            .Where(o => o.Kind == ObservationKinds.NamespaceDeclared)
            .Select(o => o.Payload["name"])
            .ToArray();

        Assert.Contains("Reference", namespaces);
        Assert.Contains("Reference.Domain", namespaces);
        Assert.Contains("Reference.Domain.Values", namespaces);

        var child = model.Observations.Single(o =>
            o.Kind == ObservationKinds.NamespaceDeclared
            && o.Payload["name"] == "Reference.Domain.Values");

        Assert.Equal(
            KnowledgeId.ForNamespace("Reference.Domain").Value,
            child.Payload["parentId"]);
    }

    [Fact]
    public async Task Todo_tipo_tem_localizacao()
    {
        var model = await CompileAsync();

        foreach (var type in Types(model))
        {
            Assert.Contains(model.Observations, o =>
                o.Kind == ObservationKinds.TypeLocation && o.Subject.Equals(type.Subject));
        }
    }

    [Fact]
    public async Task Identidade_de_tipo_inclui_o_projeto()
    {
        var model = await CompileAsync();

        Assert.All(Types(model), o =>
            Assert.Contains('@', o.Subject.Value));
    }

    [Fact]
    public async Task Projecao_e_publicada_por_projeto_com_indice()
    {
        var output = Path.Combine(_fixture.Root, "out-" + Guid.NewGuid().ToString("n"));

        await KnowledgeCompiler.CompileAsync(_fixture.SolutionPath, output);

        var types = Path.Combine(output, "Structure", "Types");

        Assert.True(File.Exists(Path.Combine(types, "INDEX.md")));
        Assert.True(File.Exists(Path.Combine(types, "Domain.md")));
        Assert.True(File.Exists(Path.Combine(output, "Structure", "Namespaces.md")));

        // O índice não repete o conteúdo: aponta para onde procurar.
        var index = await File.ReadAllTextAsync(Path.Combine(types, "INDEX.md"));

        Assert.DoesNotContain("IOrderPolicy", index, StringComparison.Ordinal);
        Assert.Contains("Domain.md", index, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Nivel_de_aquisicao_e_declarado_por_item()
    {
        var model = await CompileAsync();

        // C01 e C02 leem apenas .sln e .csproj: nada ali depende de semântica,
        // mesmo quando ela está disponível. Capacidades que consomem símbolos
        // declaram S — verificar isso pela lista de capacidades quebraria a
        // cada capacidade nova.
        Assert.All(
            model.Observations.Where(o => o.Provenance.Capability is "C01" or "C02"),
            o => Assert.Equal(AcquisitionLevel.Syntactic, o.Provenance.AcquisitionLevel));
    }

    [Fact]
    public async Task Compilacoes_repetidas_continuam_byte_identicas_com_C03()
    {
        var first = Path.Combine(_fixture.Root, "out-" + Guid.NewGuid().ToString("n"));
        var second = Path.Combine(_fixture.Root, "out-" + Guid.NewGuid().ToString("n"));

        await KnowledgeCompiler.CompileAsync(_fixture.SolutionPath, first);
        await KnowledgeCompiler.CompileAsync(_fixture.SolutionPath, second);

        foreach (var file in Directory.EnumerateFiles(first, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(first, file);

            Assert.True(
                File.ReadAllBytes(file).SequenceEqual(
                    File.ReadAllBytes(Path.Combine(second, relative))),
                $"Arquivo divergente: {relative}");
        }
    }

    [Theory]
    [InlineData(@"Erro em C:\Temp\sln\src\A\A.csproj", @"C:\Temp\sln", "Erro em src/A/A.csproj")]
    [InlineData(@"Ver D:\Outro\B.csproj", @"C:\Temp\sln", "Ver <caminho>")]
    [InlineData("Sem caminho nenhum", @"C:\Temp\sln", "Sem caminho nenhum")]
    public void Mensagem_externa_perde_caminho_absoluto(string entrada, string raiz, string esperado)
        => Assert.Equal(esperado, PathNormalizer.Sanitize(entrada, raiz));

    [Fact]
    public async Task Nenhuma_limitacao_publica_caminho_absoluto()
    {
        var model = await CompileAsync();

        // IV-08 já roda na compilação; este teste torna a intenção explícita,
        // porque a origem do texto é externa e fora do nosso controle.
        Assert.All(
            model.Observations.Where(o => o.Kind == ObservationKinds.AcquisitionLimitation),
            o => Assert.DoesNotContain(":/", o.Payload["reason"]!, StringComparison.Ordinal));
    }
}
