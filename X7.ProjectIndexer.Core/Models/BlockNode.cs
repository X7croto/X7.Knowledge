using System.Xml.Linq;
using X7.ProjectIndexer.Core.Models.Relations;

namespace X7.ProjectIndexer.Core.Models;

public sealed class BlockNode
{
    public List<InvocationNode> Invocations { get; } = [];

    public List<LocalVariableNode> LocalVariables { get; } = [];

    public List<ObjectCreationNode> ObjectCreations { get; } = [];

    public List<ReturnNode> Returns { get; } = [];

    public List<AssignmentNode> Assignments { get; } = [];

    public List<IfNode> Ifs { get; } = [];

    public List<LoopNode> Loops { get; } = [];

    public List<MemberAccessNode> MemberAccesses { get; } = [];

    public List<IdentifierNode> Identifiers { get; } = [];
}