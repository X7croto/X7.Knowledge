# Architecture

## Overview

- Projects: 8
- Types: 254
- Methods: 270
- Dependencies: 0
- Namespaces: 39

## Architecture Patterns

- Dependency Injection
- CQRS

## Layers

- Unknown (4 services)

## Services

### GraphQueryService

- Kind: Service
- Layer: Unknown
- Confidence: 85%
Classification:
- Type name ends with Service.
- Namespace indicates Services.
- Concrete type.
- Contains methods.

### IGraphQueryService

- Kind: Service
- Layer: Unknown
- Confidence: 85%
Classification:
- Type name ends with Service.
- Namespace indicates Services.
- Concrete type.
- Contains methods.

### ImpactQueryService

- Kind: Service
- Layer: Unknown
- Confidence: 85%
Classification:
- Type name ends with Service.
- Namespace indicates Services.
- Concrete type.
- Contains methods.

### IntegrityValidator

- Kind: Validator
- Layer: Unknown
- Confidence: 80%
Classification:
- Type name ends with Validator.
- Concrete type.
- Contains methods.

## Features

## Most Coupled Types

- CommandLineOptions | FanIn=0 FanOut=0
- CommandLineParser | FanIn=0 FanOut=0
- IFileScanner | FanIn=0 FanOut=0
- IOutputWriter | FanIn=0 FanOut=0
- IParser | FanIn=0 FanOut=0
- IProjectIndexer | FanIn=0 FanOut=0
- IntegrityIssue | FanIn=0 FanOut=0
- IntegritySeverity | FanIn=0 FanOut=0
- IntegrityValidationContext | FanIn=0 FanOut=0
- IntegrityValidationResult | FanIn=0 FanOut=0
- IntegrityValidationRule | FanIn=0 FanOut=0
- IntegrityValidator | FanIn=0 FanOut=0
- AssignmentNode | FanIn=0 FanOut=0
- BlockNode | FanIn=0 FanOut=0
- FieldNode | FanIn=0 FanOut=0
- IdentifierNode | FanIn=0 FanOut=0
- IfNode | FanIn=0 FanOut=0
- LocalVariableNode | FanIn=0 FanOut=0
- LoopNode | FanIn=0 FanOut=0
- MemberAccessNode | FanIn=0 FanOut=0
- MethodNode | FanIn=0 FanOut=0
- ObjectCreationNode | FanIn=0 FanOut=0
- ParameterNode | FanIn=0 FanOut=0
- PipelineContext | FanIn=0 FanOut=0
- ProjectIndex | FanIn=0 FanOut=0

## Most Called Methods

- DependencyChain.Add (292)
- MarkdownWriter.Line (100)
- MarkdownWriter.Bullet (80)
- MarkdownWriter.ToString (76)
- MarkdownWriter.H2 (44)
- CompilationUnitVisitor.Visit (42)
- DependencyChain.AddRange (36)
- IntegrityValidationContext.Error (26)
- KnowledgeExporter.Export (20)
- CycleAnalyzer.Visit (18)
- MarkdownWriter.H1 (16)
- TypeNameResolver.Resolve (14)
- KnowledgeBuilder.Build (14)
- SymbolLookup.Find (14)
- GraphBuilder.Build (12)
- AnalysisPipeline.Analyze (12)
- ScopeResolver.GetScope (12)
- DependencyResolver.AddDependency (10)
- FeatureCatalogQuery.Execute (8)
- KnowledgeQueryBuilder.Build (8)
- TypeVisitor.VisitType (8)
- DuplicateMemberReferenceRule.Validate (6)
- SymbolExtensions.GetProperty (6)
- MarkdownWriter.H3 (6)
- IntegrityValidationContext.AddIssue (4)

## Dead Code

- ArchitectureExporter.Export
- ArchitectureQueries.Abstract
- ArchitectureQueries.Concrete
- ArchitectureQueries.DistanceGreaterThan
- ArchitectureQueries.HighlyCoupled
- ArchitectureStyleRule.Analyze
- BindingDiagnostics.Print
- BrokenReferenceRule.Execute
- BusinessRuleCompiler.Build
- CallHierarchyAnalyzer.Analyze
- ClaudeExporter.Export
- CompositionResolver.Resolve
- ConsoleWriter.Write
- CycleAnalyzer.Analyze
- DeadCodeAnalyzer.Analyze
- DependencyChainQuery.Analyze
- DependencyResolver.Resolve
- DependencyRule.Analyze
- DuplicateMemberReferenceRule.Execute
- EntryPointAnalyzer.Analyze
- EntrypointCompiler.Build
- ExecutionKnowledgeBuilder.Build
- FanInOutAnalyzer.Analyze
- FeatureIndexBuilder.Build
- FeatureRule.Analyze
- FileScanner.Scan
- FlowCompiler.Build
- FlowRule.Analyze
- FlowRule.IsEntryPoint
- GraphAlgorithms.BreadthFirstSearch
- GraphExporter.Export
- GraphQueryService.GetAffectedMethods
- GraphQueryService.GetAffectedTypes
- GraphQueryService.GetCallees
- GraphQueryService.GetCallers
- GraphQueryService.GetDependencies
- GraphQueryService.GetDependents
- GraphQueryService.GetReachableTypes
- GraphQueryService.GetShortestMethodPath
- HotspotCompiler.Build
- IAnalyzer.Analyze
- IArchitectureRule.Analyze
- IFieldRule.Visit
- IGraphQueryService.GetAffectedMethods
- IGraphQueryService.GetAffectedTypes
- IGraphQueryService.GetCallees
- IGraphQueryService.GetCallers
- IGraphQueryService.GetDependencies
- IGraphQueryService.GetDependents
- IGraphQueryService.GetReachableMethods
- IGraphQueryService.GetReachableTypes
- IGraphQueryService.GetShortestMethodPath
- IKnowledgeExporter.Export
- IKnowledgeQuery.Execute
- IMethodRule.Analyze
- ImpactAnalyzer.Analyze
- ImpactQueryService.Analyze
- ImplementationResolver.Resolve
- InheritanceResolver.Resolve
- InstabilityAnalyzer.Analyze
- IntegrityValidationContext.Warning
- IOutputWriter.Write
- IParser.Parse
- IPipelineStep.Execute
- IProjectIndexer.Index
- IProjectRule.Analyze
- IPropertyRule.Visit
- IRelationshipResolver.Resolve
- ISymbolResolver.ResolveField
- ISymbolResolver.ResolveMethod
- ISymbolResolver.ResolveMethod
- ISymbolResolver.ResolveProperty
- ISymbolResolver.ResolveTypeDetailed
- ISymbolResolver.ResolveVariableType
- ITypeRule.Analyze
- KnowledgeAnalysisEngine.Execute
- KnowledgeAnalysisEngine.Register
- KnowledgeGenerationReport.Export
- KnowledgeIndexExporter.Export
- KnowledgeInferencePipeline.Infer
- KnowledgeQueries.Features
- KnowledgeQueries.Layers
- KnowledgeQueries.Patterns
- KnowledgeQueries.Services
- LayerAnalyzer.Analyze
- LayerCatalogQuery.Execute
- LayerDependencyRule.Analyze
- LayerRule.Analyze
- LocalVariableResolver.Resolve
- MarkdownWriter.Code
- MethodCallResolver.Resolve
- MethodExporter.Export
- MethodIndexBuilder.Build
- MethodScopeConsistencyRule.Execute
- MetricsAnalyzer.Analyze
- ModuleCompiler.Build
- NameResolver.ResolveField
- NameResolver.ResolveMethod
- NameResolver.ResolveMethod
- NameResolver.ResolveProperty
- NameResolver.ResolveType
- NameResolver.ResolveVariableType
- NamespaceExporter.Export
- NamespaceIndexBuilder.Build
- ParsedTypeIndexBuilder.Build
- PatternCatalogQuery.Execute
- PatternRule.Analyze
- PipelineContext.Get
- PipelineContext.Has
- PipelineContext.Set
- ProjectExporter.Export
- ProjectIndexer.Index
- QueryExtensions.DeadCode
- QueryExtensions.EntryPoints
- QueryExtensions.Layer
- QueryExtensions.Leaves
- QueryExtensions.Recursive
- QueryExtensions.Roots
- QueryExtensions.Stable
- QueryExtensions.Unstable
- RelationshipBuilder.Build
- RoslynParser.Parse
- ScopeBuilder.Build
- ScopeResolver.Resolve
- ScopeResolver.ResolveField
- ScopeResolver.ResolveLocal
- ScopeResolver.ResolveParameter
- ScopeResolver.ResolveProperty
- ScopeResolver.ResolveVisibleSymbol
- SemanticBuilder.Build
- SemanticIndexBuilder.Build
- ServiceCatalogQuery.Execute
- ServiceCompiler.Build
- SolutionExporter.Export
- SymbolExtensions.GetStringProperty
- SymbolExtensions.HasProperty
- SymbolLookup.Find
- SymbolLookup.FindAll
- SymbolLookup.FindMethod
- TypeBinder.Bind
- TypeCouplingAnalyzer.Analyze
- TypeExporter.Export
- TypeIndexBuilder.Build
- TypeNameNormalizer.Normalize
- TypeReferenceResolver.Resolve
- UnitTest1.Test1
