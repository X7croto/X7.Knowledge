using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using X7.Knowledge;
using X7.Knowledge.Model;
using Xunit;

namespace X7.KnowledgeTests;

/// <summary>
/// C05, critério 2 — toda assinatura publicada é semanticamente correta e
/// verificável contra o compilador de referência.
/// </summary>
/// <remarks>
/// **Por que existe.** O critério 1 do C05 — *o comportamento público é
/// compreensível sem abrir código* — não virou invariante: não há equivalente
/// da IV-14 para membro, porque tipo sem membro é legítimo e o modelo não
/// sabe o que ficou de fora (ADR-039 §6). A verificação mudou de lugar, e
/// este é o lugar.
///
/// **Por que a âncora é uma segunda compilação.** Reconstruir a assinatura a
/// partir das mesmas Observations seria o Producer conferindo a si mesmo:
/// passaria com o defeito dentro. Aqui os mesmos arquivos são lidos de novo e
/// montados pelo Roslyn por um caminho que não compartilha código com a
/// aquisição do X7.Knowledge.
///
/// **O que não se verifica.** Texto formatado contra texto formatado. Dois
/// formatadores divergem em espaço, ordem de acessor e posição da cláusula
/// `where`, e normalizar até coincidirem transformaria a conferência em
/// tautologia. Verificam-se três propriedades independentes: a assinatura é
/// C# válido, nada público foi omitido, nada publicado foi inventado.
///
/// Operador, indexador e implementação explícita não têm nome de
/// identificador e ficam fora das duas conferências por nome; são cobertos
/// pela análise sintática e pelos testes da fatia B.
/// </remarks>
public sealed class SignatureConformanceTests : IClassFixture<SolutionFixture>
{
    private readonly SolutionFixture _fixture;

    public SignatureConformanceTests(SolutionFixture fixture) => _fixture = fixture;

    private static readonly string[] Published =
    [
        TypeVocabulary.Public, TypeVocabulary.Protected, TypeVocabulary.ProtectedInternal
    ];

    // ------------------------------------------------------------ referência

    /// <summary>
    /// Compilação independente: os mesmos arquivos, lidos de novo, montados
    /// pelo compilador de referência.
    /// </summary>
    /// <remarks>
    /// Os projetos da fixture entram numa compilação só. Para colher símbolos
    /// isso basta, e a conferência não depende de saber de qual projeto cada
    /// tipo veio: a localização do arquivo publicado sai do nome dele, que é
    /// função da identidade do tipo (ADR-040).
    /// </remarks>
    private CSharpCompilation Reference()
    {
        var sources = Directory
            .EnumerateFiles(_fixture.Root, "*.cs", SearchOption.AllDirectories)
            .Where(Observable)
            .OrderBy(f => f, StringComparer.Ordinal)
            .Select(f => CSharpSyntaxTree.ParseText(
                File.ReadAllText(f),
                new CSharpParseOptions(LanguageVersion.Preview),
                f));

        var platform = (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? string.Empty)
            .Split(Path.PathSeparator)
            .Where(p => p.Length > 0)
            .Select(p => MetadataReference.CreateFromFile(p));

        return CSharpCompilation.Create(
            "Referencia",
            sources,
            platform,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));
    }

    /// <summary>Mesma fronteira da ADR-041, aplicada aqui de novo.</summary>
    private static bool Observable(string path)
    {
        var parts = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return !parts.Contains("obj") && !parts.Contains("bin");
    }

    private static IEnumerable<INamedTypeSymbol> Types(INamespaceOrTypeSymbol symbol)
    {
        foreach (var member in symbol.GetMembers())
        {
            switch (member)
            {
                case INamespaceSymbol child:
                    foreach (var found in Types(child))
                        yield return found;

                    break;

                case INamedTypeSymbol type:
                    yield return type;

                    foreach (var nested in Types(type))
                        yield return nested;

                    break;
            }
        }
    }

    /// <summary>
    /// Membros que a projeção deve publicar e cujo nome é identificador.
    /// </summary>
    private static IEnumerable<string> NamedSurface(INamedTypeSymbol type)
        => type.GetMembers()
            .Where(m => !m.IsImplicitlyDeclared)
            .Where(m => Published.Contains(AccessibilityOf(m), StringComparer.Ordinal))
            .Where(Nameable)
            .Select(m => m.Name)
            .Distinct(StringComparer.Ordinal);

    private static bool Nameable(ISymbol member) => member switch
    {
        IMethodSymbol method => method.MethodKind is MethodKind.Ordinary,
        IPropertySymbol property => !property.IsIndexer,
        IFieldSymbol => true,
        IEventSymbol => true,
        _ => false
    };

    private static string AccessibilityOf(ISymbol member) => member.DeclaredAccessibility switch
    {
        Accessibility.Public => TypeVocabulary.Public,
        Accessibility.Protected => TypeVocabulary.Protected,
        Accessibility.ProtectedOrInternal => TypeVocabulary.ProtectedInternal,
        _ => "outro"
    };

    /// <summary>Mesma convenção da ADR-040, derivada aqui de novo.</summary>
    private static string FileNameOf(INamedTypeSymbol type)
    {
        var parts = new List<string>();

        for (var atual = type; atual is not null; atual = atual.ContainingType)
            parts.Insert(0, atual.MetadataName);

        var espaco = type.ContainingNamespace.ToDisplayString();

        var qualificado = string.Join('+', parts);

        return (string.IsNullOrEmpty(espaco) ? qualificado : espaco + "." + qualificado)
            .Replace('`', '-');
    }

    // ------------------------------------------------------------- publicado

    private async Task<(KnowledgeModel Model, string Output)> CompileAsync()
    {
        var output = Path.Combine(_fixture.Root, "conf-" + Guid.NewGuid().ToString("n"));

        var model = await KnowledgeCompiler.CompileAsync(_fixture.SolutionPath, output);

        return (model, output);
    }

    /// <summary>
    /// Assinaturas publicadas, indexadas pelo nome do arquivo — que é o nome
    /// qualificado do tipo. Indexar por nome de arquivo, e não por pasta de
    /// projeto, evita que a conferência precise adivinhar a que projeto cada
    /// tipo pertence.
    /// </summary>
    private static Dictionary<string, List<string>> Assinaturas(string output)
    {
        var publicado = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        var root = Path.Combine(output, "Behavior");

        if (!Directory.Exists(root))
            return publicado;

        foreach (var file in Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories)
                     .OrderBy(f => f, StringComparer.Ordinal))
        {
            if (Path.GetFileName(file) == "INDEX.md")
                continue;

            var chave = Path.GetFileNameWithoutExtension(file);

            var assinaturas = new List<string>();

            foreach (var line in File.ReadAllLines(file))
            {
                if (!line.StartsWith("- `", StringComparison.Ordinal))
                    continue;

                var end = line.IndexOf('`', 3);

                if (end > 3)
                    assinaturas.Add(line[3..end]);
            }

            publicado[chave] = assinaturas;
        }

        return publicado;
    }

    /// <summary>
    /// Fecha a declaração como a linguagem exige, sem alterar o que foi
    /// publicado: corpo onde há lista de parâmetros — sem ele o construtor
    /// não analisa —, ponto e vírgula onde o membro é acessado pelo nome,
    /// nada onde já existe bloco de acessores.
    /// </summary>
    private static string Close(string signature)
    {
        if (signature.EndsWith('}'))
            return signature;

        return signature.Contains('(', StringComparison.Ordinal)
            ? signature + " { }"
            : signature + ";";
    }

    /// <summary>
    /// O tipo que envolve a declaração leva o nome do tipo que a declara: sem
    /// isso um construtor publicado seria analisado como método sem tipo de
    /// retorno.
    /// </summary>
    private static SyntaxTree Parse(string container, string signature)
        => CSharpSyntaxTree.ParseText(
            $"class {container} {{ {Close(signature)} }}",
            new CSharpParseOptions(LanguageVersion.Preview));

    private static string ContainerOf(string qualified)
    {
        var último = qualified.Split('.', '+').Last();

        var aridade = último.IndexOf('-', StringComparison.Ordinal);

        return aridade < 0 ? último : último[..aridade];
    }

    private static IReadOnlyList<string> SyntaxErrorsOf(SyntaxTree tree)
        => tree.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(d => d.GetMessage())
            .ToArray();

    // ---------------------------------------------------------------- testes

    /// <summary>
    /// Uma assinatura que não é C# válido não descreve nada, por mais
    /// plausível que pareça em Markdown. Foi assim que `int quantity = …`
    /// sobreviveu a duas fatias.
    /// </summary>
    [Fact]
    public async Task Toda_assinatura_publicada_e_C_sharp_valido()
    {
        var (_, output) = await CompileAsync();

        var publicado = Assinaturas(output);

        Assert.NotEmpty(publicado);

        var falhas = new List<string>();
        var conferidas = 0;

        foreach (var (qualified, assinaturas) in publicado.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            var container = ContainerOf(qualified);

            foreach (var assinatura in assinaturas)
            {
                conferidas++;

                var erros = SyntaxErrorsOf(Parse(container, assinatura));

                if (erros.Count > 0)
                    falhas.Add($"{qualified}: `{assinatura}` — {string.Join("; ", erros)}");
            }
        }

        Assert.True(conferidas > 0, "Nenhuma assinatura publicada foi conferida.");
        Assert.True(falhas.Count == 0, string.Join("\n", falhas));
    }

    /// <summary>
    /// Omissão é o modo de falha que nenhum invariante pega: o modelo não sabe
    /// o que ficou de fora. Quem sabe é a segunda compilação.
    /// </summary>
    [Fact]
    public async Task Nenhum_membro_publico_fica_de_fora_da_projecao()
    {
        var (model, output) = await CompileAsync();

        if (model.Manifest.AcquisitionLevel != AcquisitionLevel.Semantic)
            return;

        var publicado = Assinaturas(output);

        var ausentes = new List<string>();

        foreach (var type in Types(Reference().Assembly.GlobalNamespace))
        {
            var esperados = NamedSurface(type).ToArray();

            if (esperados.Length == 0)
                continue;

            var chave = FileNameOf(type);

            if (!publicado.TryGetValue(chave, out var assinaturas))
            {
                ausentes.Add($"{type.ToDisplayString()}: nenhum arquivo publicado ({chave}.md)");
                continue;
            }

            // Comparação por conjunto de identificadores, e não por
            // substring: `Add` aparece dentro de `Address`, e um teste que
            // aceita isso não verifica nada.
            var nomes = assinaturas
                .Select(a => Identifier(ContainerOf(chave), a))
                .OfType<string>()
                .ToHashSet(StringComparer.Ordinal);

            foreach (var nome in esperados.Where(n => !nomes.Contains(n)))
                ausentes.Add($"{type.ToDisplayString()}.{nome}");
        }

        Assert.True(ausentes.Count == 0, string.Join("\n", ausentes));
    }

    /// <summary>A direção inversa: nada publicado pode deixar de existir.</summary>
    [Fact]
    public async Task Nenhuma_assinatura_publicada_e_inventada()
    {
        var (model, output) = await CompileAsync();

        if (model.Manifest.AcquisitionLevel != AcquisitionLevel.Semantic)
            return;

        var existentes = new HashSet<string>(StringComparer.Ordinal);

        foreach (var symbol in Types(Reference().Assembly.GlobalNamespace)
                     .SelectMany(t => t.GetMembers())
                     .Where(m => !m.IsImplicitlyDeclared))
        {
            existentes.Add(symbol.Name);

            // Implementação explícita tem nome qualificado no símbolo e nome
            // curto na projeção. As duas formas valem.
            existentes.Add(symbol.Name.Split('.').Last());
        }

        var inventadas = new List<string>();

        foreach (var (qualified, assinaturas) in Assinaturas(output).OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            var container = ContainerOf(qualified);

            foreach (var assinatura in assinaturas)
            {
                var nome = Identifier(container, assinatura);

                if (nome is null || existentes.Contains(nome) || nome == container)
                    continue;

                inventadas.Add($"{qualified}: `{assinatura}`");
            }
        }

        Assert.True(inventadas.Count == 0, string.Join("\n", inventadas));
    }

    /// <summary>
    /// O identificador declarado, quando há um. Operador, indexador e
    /// conversão não têm nome de identificador e devolvem nulo.
    /// </summary>
    private static string? Identifier(string container, string signature)
    {
        var declaracao = Parse(container, signature)
            .GetRoot()
            .DescendantNodes()
            .OfType<MemberDeclarationSyntax>()
            .Skip(1)
            .FirstOrDefault();

        return declaracao switch
        {
            ConstructorDeclarationSyntax constructor => constructor.Identifier.ValueText,
            MethodDeclarationSyntax method => method.Identifier.ValueText,
            PropertyDeclarationSyntax property => property.Identifier.ValueText,
            EventDeclarationSyntax declared => declared.Identifier.ValueText,
            EventFieldDeclarationSyntax field
                => field.Declaration.Variables.FirstOrDefault()?.Identifier.ValueText,
            FieldDeclarationSyntax field
                => field.Declaration.Variables.FirstOrDefault()?.Identifier.ValueText,
            _ => null
        };
    }
}
