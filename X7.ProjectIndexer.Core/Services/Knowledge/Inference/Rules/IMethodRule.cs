using X7.ProjectIndexer.Core.Models.Symbols;
using X7.ProjectIndexer.Core.Services.Knowledge.Inference.Engine;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Inference.Rules;

public interface IMethodRule
{
    void Analyze(
        MethodSymbol method,
        InferenceContext context);
}