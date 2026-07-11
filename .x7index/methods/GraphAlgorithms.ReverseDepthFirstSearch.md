# ReverseDepthFirstSearch

Type: GraphAlgorithms
Return: HashSet<T>

## Parameters

- T root
- Func<T, IEnumerable<T>> parents

## Calls

- DependencyChain.Add
- CycleAnalyzer.Visit

## Called By

- GraphQueryService.GetAffectedMethods
- GraphQueryService.GetAffectedTypes
