using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Parsing;

public interface IParser
{
    void Parse(SourceFile file);
}