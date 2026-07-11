# Visit

Type: CycleAnalyzer
Return: void

## Parameters

- ProjectIndex index
- MethodSymbol current
- Stack<MethodSymbol> stack
- HashSet<MethodSymbol> visited

## Calls

- DependencyChain.Add
- CycleAnalyzer.Visit

## Called By

- CycleAnalyzer.Analyze
- GraphAlgorithms.DepthFirstSearch
- KnowledgeAnalysisEngine.Execute
- RoslynParser.Parse
- GraphAlgorithms.ReverseDepthFirstSearch
- CycleAnalyzer.Visit
