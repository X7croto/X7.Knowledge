# CreateTypeSymbol

Type: SemanticBuilder
Return: TypeSymbol

## Parameters

- SymbolTable semantic
- SourceFile file
- TypeNode node

## Calls

- DependencyChain.Add
- DependencyChain.AddRange
- SemanticBuilder.CreateFieldSymbol
- SemanticBuilder.CreateMethodSymbol
- SemanticBuilder.CreatePropertySymbol

## Called By

- SemanticBuilder.Build
