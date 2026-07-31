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

        Write("src/Domain/Order.cs",
            """
            namespace Reference.Domain;

            public class Order
            {
                public sealed class Line { }
            }

            public interface IOrderPolicy { }
            """);

        Write("src/Domain/Money.cs",
            """
            namespace Reference.Domain.Values;

            public readonly record struct Money;
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
