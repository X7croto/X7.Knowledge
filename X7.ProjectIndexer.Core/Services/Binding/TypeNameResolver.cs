using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Binding;

public sealed class TypeNameResolver
{
    private readonly FileBindingContext _context;

    public TypeNameResolver(FileBindingContext context)
    {
        _context = context;
    }

    public TypeReference Resolve(string name)
    {
        var result = new TypeReference
        {   
            OriginalText = name
        };

        //---------------------------------------
        // Fully qualified
        //---------------------------------------

        if (_context.Index.ParsedTypesByFullName.TryGetValue(name, out var exact))
        {
            result.Resolved = true;
            result.QualifiedName = exact.Id;

            return result;
        }

        //---------------------------------------
        // Simple name
        //---------------------------------------

        if (!_context.Index.ParsedTypesByName.TryGetValue(name, out var candidates))
            return result;

        ResolveCandidates(result, candidates);

        return result;
    }

    private void ResolveCandidates(
        TypeReference result,
        IEnumerable<TypeNode> candidates)
    {
        var sameNamespace =
            candidates.FirstOrDefault(x =>
                x.Namespace == _context.File.Namespace);

        if (sameNamespace is not null)
        {
            result.Resolved = true;
            result.QualifiedName = sameNamespace.Id;
            return;
        }

        foreach (var ns in _context.ImportedNamespaces)
        {
            var match =
                candidates.FirstOrDefault(x =>
                    x.Namespace == ns);

            if (match is not null)
            {
                result.Resolved = true;
                result.QualifiedName = match.Id;
                return;
            }
        }

        foreach (var candidate in candidates)
            result.Candidates.Add(candidate.Id);

        result.Ambiguous =
            result.Candidates.Count > 1;

        if (!result.Ambiguous &&
            result.Candidates.Count == 1)
        {
            result.Resolved = true;
            result.QualifiedName =
                result.Candidates[0];
        }
    }
}