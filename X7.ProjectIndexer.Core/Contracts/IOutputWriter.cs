using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Contracts;

public interface IOutputWriter
{
    void Write(ProjectIndexOld index, string outputDirectory);
}