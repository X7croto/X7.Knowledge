using X7.ProjectIndexer.Core.Models;
using X7.ProjectIndexer.Core.Models.Knowledge;

namespace X7.ProjectIndexer.Core.Services.Knowledge;

public sealed class HotspotCompiler
{
    public void Build(ProjectIndexOld index)
    {
        index.Knowledge.Quality.Hotspots.Clear();

        foreach (var type in index.Semantic.Types
                     .OrderByDescending(x => x.FanIn + x.FanOut)
                     .Take(50))
        {
            index.Knowledge.Quality.Hotspots.Add(new HotspotModel
            {
                TypeId = type.Id,
                Name = type.Name,
                Namespace = type.Namespace,
                Score = type.FanIn + type.FanOut
            });
        }
    }
}