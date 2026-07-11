using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Graph;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Graph;

public sealed class GraphBuilder
{
    public void Build(ProjectIndexOld index)
    {
        var graph = index.Graph;
        var semantic = index.Semantic;

        graph.Nodes.Clear();
        graph.Edges.Clear();

        //--------------------------------------
        // Nodes
        //--------------------------------------

        foreach (var project in semantic.Projects)
        {
            graph.Nodes.Add(new GraphNode
            {
                Id = project.Id,
                Kind = "Project",
                Symbol = project
            });
        }

        foreach (var type in semantic.Types)
        {
            graph.Nodes.Add(new GraphNode
            {
                Id = type.Id,
                Kind = "Type",
                Symbol = type
            });
        }

        foreach (var method in semantic.Methods)
        {
            graph.Nodes.Add(new GraphNode
            {
                Id = method.Id,
                Kind = "Method",
                Symbol = method
            });
        }

        foreach (var property in semantic.Properties)
        {
            graph.Nodes.Add(new GraphNode
            {
                Id = property.Id,
                Kind = "Property",
                Symbol = property
            });
        }

        foreach (var field in semantic.Fields)
        {
            graph.Nodes.Add(new GraphNode
            {
                Id = field.Id,
                Kind = "Field",
                Symbol = field
            });
        }

        //--------------------------------------
        // Contains
        //--------------------------------------

        foreach (var project in semantic.Projects)
        {
            foreach (var type in project.Types)
            {
                graph.Edges.Add(new GraphEdge
                {
                    SourceId = project.Id,
                    TargetId = type.Id,
                    Relation = "Contains"
                });

                foreach (var method in type.Methods)
                {
                    graph.Edges.Add(new GraphEdge
                    {
                        SourceId = type.Id,
                        TargetId = method.Id,
                        Relation = "Contains"
                    });
                }

                foreach (var property in type.Properties)
                {
                    graph.Edges.Add(new GraphEdge
                    {
                        SourceId = type.Id,
                        TargetId = property.Id,
                        Relation = "Contains"
                    });
                }

                foreach (var field in type.Fields)
                {
                    graph.Edges.Add(new GraphEdge
                    {
                        SourceId = type.Id,
                        TargetId = field.Id,
                        Relation = "Contains"
                    });
                }
            }
        }

        //--------------------------------------
        // Calls
        //--------------------------------------

        foreach (var call in semantic.Calls)
        {
            if (call.Callee is null)
                continue;

            graph.Edges.Add(new GraphEdge
            {
                SourceId = call.Caller.Id,
                TargetId = call.Callee.Id,
                Relation = "Calls"
            });
        }

        //--------------------------------------
        // Member Access
        //--------------------------------------

        foreach (var method in semantic.Methods)
        {
            foreach (var access in method.Body.MemberAccesses)
            {
                if (access.TargetSymbol is null)
                    continue;

                graph.Edges.Add(new GraphEdge
                {
                    SourceId = method.Id,
                    TargetId = GetSymbolId(access.TargetSymbol),
                    Relation = "Uses"
                });
            }
        }

        //--------------------------------------
        // Inheritance
        //--------------------------------------

        foreach (var inheritance in semantic.Inheritances)
        {
            graph.Edges.Add(new GraphEdge
            {
                SourceId = inheritance.Child.Id,
                TargetId = inheritance.Parent.Id,
                Relation = "Inherits"
            });
        }

        //--------------------------------------
        // Implementation
        //--------------------------------------

        foreach (var implementation in semantic.Implementations)
        {
            graph.Edges.Add(new GraphEdge
            {
                SourceId = implementation.Type.Id,
                TargetId = implementation.Interface.Id,
                Relation = "Implements"
            });
        }

        //--------------------------------------
        // Dependencies
        //--------------------------------------

        foreach (var dependency in semantic.Dependencies)
        {
            graph.Edges.Add(new GraphEdge
            {
                SourceId = dependency.Source.Id,
                TargetId = dependency.Target.Id,
                Relation = "DependsOn"
            });
        }

        //--------------------------------------
        // Composition
        //--------------------------------------

        foreach (var composition in semantic.Compositions)
        {
            graph.Edges.Add(new GraphEdge
            {
                SourceId = composition.Owner.Id,
                TargetId = composition.Part.Id,
                Relation = "Composition"
            });
        }

        //--------------------------------------
        // Aggregation
        //--------------------------------------

        foreach (var aggregation in semantic.Aggregations)
        {
            graph.Edges.Add(new GraphEdge
            {
                SourceId = aggregation.Owner.Id,
                TargetId = aggregation.Part.Id,
                Relation = "Aggregation"
            });
        }
    }

    private static string GetSymbolId(ISymbol symbol)
    {
        return symbol switch
        {
            TypeSymbol x => x.Id,
            MethodSymbol x => x.Id,
            PropertySymbol x => x.Id,
            FieldSymbol x => x.Id,
            _ => ""
        };
    }
}