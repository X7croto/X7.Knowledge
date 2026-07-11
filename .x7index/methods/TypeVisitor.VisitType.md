# VisitType

Type: TypeVisitor
Return: void

## Parameters

- TypeDeclarationSyntax declaration
- string kind

## Calls

- DependencyChain.Add
- TypeVisitor.GetAccessibility
- MarkdownWriter.ToString
- CompilationUnitVisitor.Visit

## Called By

- TypeVisitor.VisitClassDeclaration
- TypeVisitor.VisitInterfaceDeclaration
- TypeVisitor.VisitRecordDeclaration
- TypeVisitor.VisitStructDeclaration
