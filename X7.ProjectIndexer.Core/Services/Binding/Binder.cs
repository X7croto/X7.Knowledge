namespace X7.ProjectIndexer.Core.Services.Binding;

using X7.ProjectIndexer.Core.Models;

public sealed class Binder
{
    public void Bind(ProjectIndexOld index)
    {
        foreach (var project in index.Projects)
        {
            foreach (var file in project.Files)
            {
                var context = new FileBindingContext(index, file);

                new TypeBinder(context).Bind();
            }
        }
    }
}