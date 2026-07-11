using X7.ProjectIndexer.Core.Models.Semantic;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Semantic;

public sealed class ScopeBuilder
{
    public void Build(SymbolTable semantic)
    {
        semantic.ScopesByMethodId.Clear();

        foreach (var method in semantic.Methods)
        {
            var scope = new MethodScope
            {
                Method = method
            };

            //
            // parâmetros
            //

            foreach (var parameter in method.Parameters)
            {
                scope.Parameters[parameter.Name] = parameter;
            }

            //
            // variáveis locais
            //

            foreach (var local in method.Body.LocalVariables)
            {
                scope.LocalVariables[local.Name] = local;
            }

            //
            // membros do tipo
            //

            var type = method.DeclaringType;

            if (type is not null)
            {
                foreach (var field in type.Fields)
                    scope.Fields[field.Name] = field;

                foreach (var property in type.Properties)
                    scope.Properties[property.Name] = property;
            }

            semantic.ScopesByMethodId[method.Id] = scope;
        }
    }
}