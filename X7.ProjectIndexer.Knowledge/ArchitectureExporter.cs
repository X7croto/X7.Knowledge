using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Knowledge;

public sealed class ArchitectureExporter
{
    public void Export(ProjectIndexOld index, string folder)
    {
        var md = new MarkdownWriter();

        md.H1("Architecture");

        //----------------------------------------------------
        // Overview
        //----------------------------------------------------

        md.H2("Overview");

        md.Bullet($"Projects: {index.Semantic.Projects.Count}");
        md.Bullet($"Types: {index.Semantic.Types.Count}");
        md.Bullet($"Methods: {index.Semantic.Methods.Count}");
        md.Bullet($"Dependencies: {index.Semantic.Dependencies.Count}");
        md.Bullet($"Namespaces: {index.Semantic.Types.Select(x => x.Namespace).Distinct().Count()}");

        md.Line();

        //----------------------------------------------------
        // Architecture Patterns
        //----------------------------------------------------

        if (index.Knowledge.Architecture.Patterns.Any())
        {
            md.H2("Architecture Patterns");

            foreach (var pattern in index.Knowledge.Architecture.Patterns)
                md.Bullet(pattern);

            md.Line();
        }

        //----------------------------------------------------
        // Layers
        //----------------------------------------------------

        md.H2("Layers");

        foreach (var layer in index.Knowledge.Architecture.Services
                     .GroupBy(x => x.Layer)
                     .OrderBy(x => x.Key))
        {
            md.Bullet($"{layer.Key} ({layer.Count()} services)");
        }

        md.Line();

        //----------------------------------------------------
        // Services
        //----------------------------------------------------

        md.H2("Services");

        foreach (var service in index.Knowledge.Architecture.Services
                     .OrderBy(x => x.Layer)
                     .ThenBy(x => x.Name))
        {
            md.H3(service.Name);

            md.Bullet($"Kind: {service.Description.Kind}");
            md.Bullet($"Layer: {service.Layer}");
            md.Bullet($"Confidence: {service.Description.Confidence}%");

            if (service.Dependencies.Count > 0)
            {
                md.Line("Dependencies:");

                foreach (var dependency in service.Dependencies)
                    md.Bullet(dependency);
            }

            if (service.Description.Reasons.Count > 0)
            {
                md.Line("Classification:");

                foreach (var reason in service.Description.Reasons)
                    md.Bullet(reason);
            }

            md.Line();
        }

        md.H2("Features");

        foreach (var feature in index.Knowledge.Architecture.Features)
        {
            md.H3(feature.Name);

            md.Line("Types:");

            foreach (var type in feature.Types.OrderBy(x => x.Name))
                md.Bullet(type.Name);

            md.Line();

            md.Line("Methods:");

            foreach (var method in feature.Methods)
                md.Bullet($"{method.DeclaringType?.Name}.{method.Name}");

            md.Line();
        }
        //----------------------------------------------------
        // Flows
        //----------------------------------------------------

        if (index.Knowledge.Architecture.Flows.Any())
        {
            md.H2("Execution Flows");

            foreach (var flow in index.Knowledge.Architecture.Flows)
            {
                md.H3(flow.Name);

                foreach (var step in flow.Steps.OrderBy(x => x.Order))
                {
                    md.Bullet(
                        $"{step.Order}. {step.Method.DeclaringType?.Name}.{step.Method.Name} [{step.Role}]");
                }

                md.Line();
            }
        }

        //----------------------------------------------------
        // Most Coupled Types
        //----------------------------------------------------

        md.H2("Most Coupled Types");

        foreach (var type in index.Semantic.Types
                     .OrderByDescending(x => x.FanIn + x.FanOut)
                     .Take(25))
        {
            md.Bullet(
                $"{type.Name} | FanIn={type.FanIn} FanOut={type.FanOut}");
        }

        md.Line();

        //----------------------------------------------------
        // Most Called Methods
        //----------------------------------------------------

        md.H2("Most Called Methods");

        foreach (var method in index.Semantic.Methods
                     .OrderByDescending(x => x.FanIn)
                     .Take(25))
        {
            md.Bullet(
                $"{method.DeclaringType?.Name}.{method.Name} ({method.FanIn})");
        }

        md.Line();

        //----------------------------------------------------
        // Dead Code
        //----------------------------------------------------

        md.H2("Dead Code");

        foreach (var method in index.Semantic.Methods
                     .Where(x => x.IsDeadCode)
                     .OrderBy(x => x.DeclaringType?.Name)
                     .ThenBy(x => x.Name))
        {
            md.Bullet($"{method.DeclaringType?.Name}.{method.Name}");
        }

        File.WriteAllText(
            Path.Combine(folder, "architecture.md"),
            md.ToString());
    }
}