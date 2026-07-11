using Microsoft.CodeAnalysis;
using System.Xml.Linq;
using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Semantic;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Semantic;

public sealed class SemanticBuilder
{
    public void Build(ProjectIndexOld index)
    {
        var semantic = index.Semantic;

        foreach (var project in index.Projects)
        {
            var projectSymbol = new ProjectSymbol
            {
                Id = project.Name,
                Name = project.Name,
                Path = project.ProjectFile
            };

            semantic.Projects.Add(projectSymbol);

            foreach (var file in project.Files)
            {
                foreach (var type in file.Types)
                {
                    var typeSymbol = CreateTypeSymbol(semantic, file, type);

                    projectSymbol.Types.Add(typeSymbol);
                    semantic.Types.Add(typeSymbol);
                    semantic.TypesById[typeSymbol.Id] = typeSymbol;
                    semantic.TypesByName[typeSymbol.Name] = typeSymbol;

                    foreach (var method in typeSymbol.Methods)
                    {
                        semantic.Methods.Add(method);
                        semantic.MethodsById[method.Id] = method;

                        if (!semantic.MethodsByName.TryGetValue(method.Name, out var list))
                        {
                            list = [];
                            semantic.MethodsByName[method.Name] = list;
                        }

                        list.Add(method);
                    }

                    foreach (var property in typeSymbol.Properties)
                    {
                        semantic.Properties.Add(property);
                        semantic.PropertiesById[property.Id] = property;
                    }

                    foreach (var field in typeSymbol.Fields)
                    {
                        semantic.Fields.Add(field);
                        semantic.FieldsById[field.Id] = field;
                    }

                    foreach (var method in typeSymbol.Methods)
                    {
                        foreach (var parameter in method.Parameters)
                            semantic.Parameters.Add(parameter);

                        foreach (var local in method.Body.LocalVariables)
                        {
                            semantic.LocalVariables.Add(local);
                        }
                    }

                }
            }
        }
        ResolveBoundTypes(index);

        Console.WriteLine();

        Console.WriteLine("===== SYMBOL TABLE =====");

        Console.WriteLine($"Projects      : {index.Semantic.Projects.Count}");

        Console.WriteLine($"Types         : {index.Semantic.Types.Count}");

        Console.WriteLine($"Methods       : {index.Semantic.Methods.Count}");

        Console.WriteLine($"Properties    : {index.Semantic.Properties.Count}");

        Console.WriteLine($"Fields        : {index.Semantic.Fields.Count}");

        Console.WriteLine($"Dependencies  : {index.Semantic.Dependencies.Count}");

        Console.WriteLine($"Calls         : {index.Semantic.Calls.Count}");

        Console.WriteLine($"References    : {index.Semantic.References.Count}");

        Console.WriteLine($"Inheritance   : {index.Semantic.Inheritances.Count}");

        Console.WriteLine($"Implement     : {index.Semantic.Implementations.Count}");

        Console.WriteLine();
    }

    private static TypeSymbol CreateTypeSymbol(SymbolTable semantic, SourceFile file, TypeNode node)
    {
        var id = $"{file.Namespace}.{node.Name}";

        var symbol = new TypeSymbol
        {
            Id = id,
            Name = node.Name,
            Namespace = file.Namespace ?? "",
            Kind = node.Kind,
            Accessibility = node.Accessibility,
            Partial = node.Partial,
            Static = node.Static,
            Abstract = node.Abstract,
            Record = node.Record,
            BaseType = node.BaseType
        };

        symbol.Interfaces.AddRange(node.Interfaces);
        symbol.Attributes.AddRange(node.Attributes);

        foreach (var method in node.Methods)
            symbol.Methods.Add(CreateMethodSymbol(symbol, method));

        foreach (var property in node.Properties)
            symbol.Properties.Add(CreatePropertySymbol(symbol, property));

        foreach (var field in node.Fields)
            symbol.Fields.Add(CreateFieldSymbol(symbol, field));

        return symbol;
    }

    private static MethodSymbol CreateMethodSymbol(TypeSymbol owner, MethodNode node)
    {
        var symbol = new MethodSymbol
        {
            Id = $"{owner.Id}.{node.Name}",
            Name = node.Name,
            ReturnType = node.ReturnType,
            Accessibility = node.Accessibility,
            Static = node.Static,
            Virtual = node.Virtual,
            Override = node.Override,
            Abstract = node.Abstract,
            Async = node.Async,
            DeclaringType = owner
        };

        //========================
        // Identifiers
        //========================

        foreach (var identifier in node.Body.Identifiers)
        {
            symbol.Body.Identifiers.Add(new IdentifierSymbol
            {
                Name = identifier.Name,
                Line = identifier.Line
            });
        }

        //========================
        // Invocations
        //========================

        foreach (var invocation in node.Body.Invocations)
        {
            symbol.Body.Invocations.Add(new InvocationSymbol
            {
                Name = invocation.Name,
                Expression = invocation.Expression,
                Line = invocation.Line
            });
        }

        //========================
        // Object Creations
        //========================

        foreach (var creation in node.Body.ObjectCreations)
        {
            symbol.Body.ObjectCreations.Add(new ObjectCreationSymbol
            {
                Type = creation.Type,
                Expression = creation.Expression,
                Line = creation.Line
            });
        }

        //========================
        // Member Access
        //========================

        foreach (var access in node.Body.MemberAccesses)
        {
            symbol.Body.MemberAccesses.Add(new MemberAccessSymbol
            {
                Expression = access.Expression,
                Member = access.Member,
                Line = access.Line
            });
        }

        //========================
        // Assignments
        //========================

        foreach (var assignment in node.Body.Assignments)
        {
            symbol.Body.Assignments.Add(new AssignmentSymbol
            {
                LeftExpression = assignment.Left,
                RightExpression = assignment.Right,
                Line = assignment.Line
            });
        }

        //========================
        // Returns
        //========================

        foreach (var ret in node.Body.Returns)
        {
            symbol.Body.Returns.Add(new ReturnSymbol
            {
                Expression = ret.Expression ?? string.Empty,
                Line = ret.Line
            });
        }

        //========================
        // Locals
        //========================

        foreach (var variable in node.Body.LocalVariables)
        {
            symbol.Body.LocalVariables.Add(new LocalVariableSymbol
            {
                Name = variable.Name,
                Type = variable.Type
            });
        }
        symbol.Attributes.AddRange(node.Attributes);

        foreach (var parameter in node.Parameters)
        {
            symbol.Parameters.Add(new ParameterSymbol
            {
                Name = parameter.Name,
                Type = parameter.Type,
                Ref = parameter.Ref,
                Out = parameter.Out,
                Params = parameter.Params,
                Optional = parameter.Optional
            });
        }

        return symbol;
    }

    private static PropertySymbol CreatePropertySymbol(TypeSymbol owner, PropertyNode node)
    {
        var symbol = new PropertySymbol
        {
            Id = $"{owner.Id}.{node.Name}",
            Name = node.Name,
            Type = node.Type,
            Accessibility = node.Accessibility,
            HasGetter = node.HasGetter,
            HasSetter = node.HasSetter,
            InitOnly = node.InitOnly
        };
        symbol.DeclaringType = owner;

        return symbol;
    }


    private static FieldSymbol CreateFieldSymbol(TypeSymbol owner, FieldNode node)
    {
        var symbol = new FieldSymbol
        {
            Id = $"{owner.Id}.{node.Name}",
            Name = node.Name,
            Type = node.Type,
            Accessibility = node.Accessibility,
            Static = node.Static,
            Readonly = node.Readonly,
            Const = node.Const
        };
        symbol.DeclaringType = owner;

        return symbol;
    }
    private static void ResolveBoundTypes(ProjectIndexOld index)
    {
        var lookup = new SymbolLookup(index.Semantic);

        foreach (var project in index.Projects)
        {
            foreach (var file in project.Files)
            {
                foreach (var typeNode in file.Types)
                {
                    if (!index.Semantic.TypesById.TryGetValue(typeNode.Id, out var type))
                        continue;

                    //----------------------------------
                    // Herança
                    //----------------------------------

                    type.BaseTypeSymbol =
                        lookup.Find(typeNode.BaseTypeReference);

                    //----------------------------------
                    // Interfaces
                    //----------------------------------

                    foreach (var iface in typeNode.InterfaceReferences)
                    {
                        var symbol = lookup.Find(iface);

                        if (symbol != null)
                            type.InterfaceSymbols.Add(symbol);
                    }

                    //----------------------------------
                    // Fields
                    //----------------------------------

                    foreach (var field in type.Fields)
                    {
                        var node =
                            typeNode.Fields.FirstOrDefault(x => x.Name == field.Name);

                        if (node is null)
                            continue;

                        field.TypeSymbol =
                            lookup.Find(node.TypeReference);
                    }

                    //----------------------------------
                    // Properties
                    //----------------------------------

                    foreach (var property in type.Properties)
                    {
                        var node =
                            typeNode.Properties.FirstOrDefault(x => x.Name == property.Name);

                        if (node is null)
                            continue;

                        property.TypeSymbol =
                            lookup.Find(node.TypeReference);
                    }

                    //----------------------------------
                    // Methods
                    //----------------------------------

                    foreach (var method in type.Methods)
                    {
                        var node =
                            typeNode.Methods.FirstOrDefault(x => x.Name == method.Name);

                        if (node is null)
                            continue;

                        method.ReturnTypeSymbol =
                            lookup.Find(node.ReturnTypeReference);

                        var parameterCount = Math.Min(method.Parameters.Count, node.Parameters.Count);

                        for (int i = 0; i < parameterCount; i++)
                        {
                            method.Parameters[i].TypeSymbol =
                                lookup.Find(node.Parameters[i].TypeReference);
                        }

                        var localCount = Math.Min(method.Body.LocalVariables.Count, node.Body.LocalVariables.Count);

                        for (int i = 0; i < localCount; i++)
                        {
                            method.Body.LocalVariables[i].TypeSymbol =
                                lookup.Find(node.Body.LocalVariables[i].TypeReference);
                        }
                    }
                }
            }
        }
    }
}