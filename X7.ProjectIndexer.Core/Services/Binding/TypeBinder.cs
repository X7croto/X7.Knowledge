using X7.ProjectIndexer.Core.Models;

namespace X7.ProjectIndexer.Core.Services.Binding;

public sealed class TypeBinder
{
    private readonly FileBindingContext _context;
    private readonly TypeNameResolver _resolver;

    public TypeBinder(FileBindingContext context)
    {
        _context = context;
        _resolver = new TypeNameResolver(context);
    }

    public void Bind()
    {
        foreach (var type in _context.File.Types)
        {
            Bind(type);
        }
    }

    private void Bind(TypeNode type)
    {
        //----------------------------------
        // Herança
        //----------------------------------

        if (!string.IsNullOrWhiteSpace(type.BaseType))
            type.BaseTypeReference =
                _resolver.Resolve(type.BaseType);

        //----------------------------------
        // Interfaces
        //----------------------------------

        foreach (var iface in type.Interfaces)
        {
            type.InterfaceReferences.Add(
                _resolver.Resolve(iface));
        }

        //----------------------------------
        // Campos
        //----------------------------------

        foreach (var field in type.Fields)
        {
            field.TypeReference =
                _resolver.Resolve(field.Type);
        }

        //----------------------------------
        // Propriedades
        //----------------------------------

        foreach (var property in type.Properties)
        {
            property.TypeReference =
                _resolver.Resolve(property.Type);
        }

        //----------------------------------
        // Métodos
        //----------------------------------

        foreach (var method in type.Methods)
        {
            method.ReturnTypeReference =
                _resolver.Resolve(method.ReturnType);

            foreach (var parameter in method.Parameters)
            {
                parameter.TypeReference =
                    _resolver.Resolve(parameter.Type);
            }

            foreach (var local in method.Body.LocalVariables)
            {
                local.TypeReference =
                    _resolver.Resolve(local.Type);
            }
        }
    }
}