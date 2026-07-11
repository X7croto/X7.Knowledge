using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Inference.Rules;

public interface IPropertyRule
{
    void Visit(
        PropertySymbol property,
        InferenceContext context);
}