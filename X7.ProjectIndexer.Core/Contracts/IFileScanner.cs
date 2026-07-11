using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Contracts;

public interface IFileScanner
{
    ProjectIndexOld Scan(string root);
}