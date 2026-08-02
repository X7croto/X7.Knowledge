namespace X7.KnowledgeTests;

/// <summary>
/// Cria uma solução de referência em disco temporário.
/// Pequena de propósito: exercita pasta aninhada, projeto solto,
/// multi-target, projeto de teste e propriedade não resolvida.
/// </summary>
public sealed class SolutionFixture : IDisposable
{
    public SolutionFixture()
    {
        Root = Path.Combine(Path.GetTempPath(), "x7k-" + Guid.NewGuid().ToString("n"));

        Directory.CreateDirectory(Root);

        Write("Reference.slnx",
            """
            <Solution>
              <Folder Name="/src/">
                <Project Path="src/Domain/Domain.csproj" />
              </Folder>
              <Folder Name="/src/Core/">
                <Project Path="src/Core/Kernel/Kernel.csproj" />
              </Folder>
              <Folder Name="/tests/">
                <Project Path="tests/Domain.Tests/Domain.Tests.csproj" />
              </Folder>
              <Project Path="tools/Cli/Cli.csproj" />
            </Solution>
            """);

        Write("src/Domain/Domain.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFrameworks>net10.0;net9.0</TargetFrameworks>
                <Nullable>enable</Nullable>
                <LangVersion>latest</LangVersion>
              </PropertyGroup>
            </Project>
            """);

        Write("src/Core/Kernel/Kernel.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <AssemblyName>$(MSBuildProjectName).Core</AssemblyName>
              </PropertyGroup>
              <ItemGroup>
                <ProjectReference Include="..\..\Domain\Domain.csproj" />
              </ItemGroup>
            </Project>
            """);

        Write("tests/Domain.Tests/Domain.Tests.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="xunit" Version="2.9.3" />
                <ProjectReference Include="..\..\src\Domain\Domain.csproj" />
              </ItemGroup>
            </Project>
            """);

        // Membros de propósito variado: sobrecarga, parâmetro opcional,
        // `out`, método genérico, propriedade `init`, membro não público e
        // tipo aninhado com superfície. É o que o C05 precisa exercitar.
        Write("src/Domain/Order.cs",
            """
            namespace Reference.Domain;

            public class Order
            {
                public Order(string number) => Number = number;

                public string Number { get; init; }

                protected virtual int Total { get; set; }

                private string Secret { get; set; } = "";

                public void Add(string sku) { }

                public void Add(string sku, int quantity = 1) { }

                public T Map<T>(string sku) where T : class, IOrderPolicy, new() => default!;

                public bool TryFind(string sku, out string found)
                {
                    found = sku;
                    return true;
                }

                internal void Recalculate() { }

                public sealed class Line
                {
                    public int Quantity { get; init; }
                }
            }

            public interface IOrderPolicy
            {
                bool Allows(Order order);
            }
            """);

        Write("src/Domain/Money.cs",
            """
            namespace Reference.Domain.Values;

            public readonly record struct Money;
            """);

        Write("src/Domain/Repository.cs",
            """
            using System;

            namespace Reference.Domain;

            public interface IRepository
            {
                void Save(object entity);
            }

            public abstract class RepositoryBase : IRepository
            {
                public abstract void Save(object entity);
            }

            public sealed class OrderRepository : RepositoryBase, IDisposable
            {
                public override void Save(object entity) { }

                public void Dispose() { }
            }

            public sealed class DomainError : Exception { }

            public interface IQuery<T> where T : notnull
            {
                T Run();
            }

            public sealed class NameQuery : IQuery<System.Collections.Generic.List<string>> { }
            """);

        // Tipo parcial em dois arquivos, variância declarada e delegate:
        // cobre os kinds de estrutura do C04 que a fixture não exercitava.
        Write("src/Domain/Catalog.cs",
            """
            namespace Reference.Domain;

            public partial class Catalog { }

            public interface IEvents<in TIn, out TOut> where TIn : struct { }

            internal delegate void Notify();
            """);

        Write("src/Domain/Catalog.Extra.cs",
            """
            namespace Reference.Domain;

            public partial class Catalog { }
            """);

        // Formas que a solucao de referencia nao tem: operador, conversao,
        // evento em forma de campo, evento com acessores, indexador,
        // construtor estatico, campo const e implementacao explicita de
        // interface. A fatia B se verifica aqui, e nao contra a referencia.
        Write("src/Domain/Ledger.cs",
            """
            using System;

            namespace Reference.Domain;

            public interface IAudit
            {
                void Record(string entry);
            }

            public sealed class Ledger : IAudit
            {
                public const string Kind = "ledger";

                private static readonly int Limit = 100;

                public event EventHandler? Changed;

                public event EventHandler? Audited
                {
                    add { }
                    remove { }
                }

                static Ledger() { }

                public string this[int index] => string.Empty;

                public static Ledger operator +(Ledger left, Ledger right) => left;

                public static implicit operator string(Ledger ledger) => Kind;

                void IAudit.Record(string entry) { }

                // ref readonly, in, e valores padrao escritos de formas
                // diferentes: `default` e `null` sao a mesma coisa nos
                // metadados e coisas diferentes na declaracao.
                public void Apply(
                    in int origem,
                    ref readonly int destino,
                    string rotulo = "x",
                    int? escala = null,
                    string? nota = default) { }
            }
            """);

        // Gerador de codigo real: o [GeneratedRegex] emite tipos dentro de
        // obj/, com `<` e `>` no nome. Foi assim que a saida de build entrou
        // na Base publicada, e e o caso que a ADR-041 fecha.
        Write("src/Domain/PathRules.cs",
            """
            using System.Text.RegularExpressions;

            namespace Reference.Domain;

            public static partial class PathRules
            {
                [GeneratedRegex("^[a-z]+$")]
                public static partial Regex LowerOnly();
            }
            """);

        Write("src/Core/Kernel/Clock.cs",
            """
            namespace Reference.Kernel;

            public static class Clock { }
            """);

        Write("tools/Cli/Cli.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <OutputType>Exe</OutputType>
              </PropertyGroup>
              <ItemGroup>
                <ProjectReference Include="..\..\src\Core\Kernel\Kernel.csproj" />
              </ItemGroup>
            </Project>
            """);
    }

    public string Root { get; }

    public string SolutionPath => Path.Combine(Root, "Reference.slnx");

    public string OutputDirectory => Path.Combine(Root, "Knowledge");

    private void Write(string relativePath, string content)
    {
        var path = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        File.WriteAllText(path, content);
    }

    public void Dispose()
    {
        if (Directory.Exists(Root))
            Directory.Delete(Root, recursive: true);
    }
}
