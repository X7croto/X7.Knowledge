# Visit

Type: ImpactAnalyzer
Return: void

## Parameters

- MethodSymbol root
- MethodSymbol current
- int distance
- HashSet<MethodSymbol> visited
- ProjectIndex index

## Calls

- DependencyChain.Add
- ImpactAnalyzer.Visit

## Called By

- ImpactAnalyzer.AnalyzeMethod
- ImpactAnalyzer.Visit
