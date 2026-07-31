using Microsoft.CodeAnalysis;
using X7.Knowledge.Model;

namespace X7.Knowledge.Acquisition.Roslyn;

/// <summary>
/// Resultado da aquisição de código de um projeto, com o nível efetivamente
/// alcançado. Nível é por projeto: uma solução pode ter parte resolvida
/// semanticamente e parte não (Constituição §5.3).
/// </summary>
public sealed record SourceCompilation
{
    public required string ProjectRelativePath { get; init; }

    public required AcquisitionLevel Level { get; init; }

    /// <summary>
    /// Disponível apenas em nível S. Qualificado por extenso: `Compilation`
    /// sozinho colide com o namespace X7.Knowledge.Compilation.
    /// </summary>
    public Microsoft.CodeAnalysis.Compilation? Compilation { get; init; }

    /// <summary>Sempre disponível: árvores sintáticas ordenadas por caminho.</summary>
    public required IReadOnlyList<SourceFile> Files { get; init; }

    public required IReadOnlyList<AcquisitionLimitation> Limitations { get; init; }
}

public sealed record SourceFile
{
    /// <summary>Caminho relativo à raiz da solução, com '/' (D-02).</summary>
    public required string RelativePath { get; init; }

    public required SyntaxTree Tree { get; init; }
}
