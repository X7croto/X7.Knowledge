# Visit

Type: FlowRule
Return: void

## Parameters

- MethodSymbol method
- FlowModel flow
- HashSet<MethodSymbol> visited
- int depth

## Calls

- DependencyChain.Add
- FlowRule.InferRole
- FlowRule.Visit

## Called By

- FlowRule.BuildFlow
- FlowRule.Visit
