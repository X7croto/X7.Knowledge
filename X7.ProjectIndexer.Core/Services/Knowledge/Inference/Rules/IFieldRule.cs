using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Knowledge.Inference.Rules;

public interface IFieldRule
{
    void Visit(
        FieldSymbol field,
        InferenceContext context);
}