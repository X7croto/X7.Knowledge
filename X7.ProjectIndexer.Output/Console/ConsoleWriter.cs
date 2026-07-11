using X7.ProjectIndexer.Core.Models;
using System;

namespace X7.ProjectIndexer.Output.Writers;

public sealed class ConsoleWriter
{
    public void Write(ProjectIndexOld index)
    {
        Console.WriteLine();

        Console.WriteLine("===== SUMMARY =====");
        Console.WriteLine($"Projects : {index.Projects.Count}");
        Console.WriteLine($"Types    : {index.Semantic.Types.Count}");
        Console.WriteLine($"Methods  : {index.Semantic.Methods.Count}");
        Console.WriteLine($"Calls    : {index.Semantic.Calls.Count}");
    }
}