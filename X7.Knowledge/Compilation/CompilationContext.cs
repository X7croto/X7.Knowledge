using X7.Knowledge.Acquisition;
using X7.Knowledge.Model;

namespace X7.Knowledge.Compilation;

/// <summary>Estado compartilhado por um único ciclo de compilação.</summary>
public sealed class CompilationContext
{
    public CompilationContext(SolutionFile solution, AcquisitionLevel level)
    {
        Solution = solution;
        AcquisitionLevel = level;
        Knowledge = new KnowledgeModelBuilder();
    }

    public SolutionFile Solution { get; }

    public AcquisitionLevel AcquisitionLevel { get; }

    public KnowledgeModelBuilder Knowledge { get; }

    public KnowledgeId SolutionId => KnowledgeId.ForSolution(Solution.Name);
}
