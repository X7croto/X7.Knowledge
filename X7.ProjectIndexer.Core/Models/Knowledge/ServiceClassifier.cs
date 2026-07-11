using X7.ProjectIndexer.Core.Models.Knowledge;
using X7.ProjectIndexer.Core.Models.Symbols;

namespace X7.ProjectIndexer.Core.Services.Knowledge;

public sealed class ServiceClassifier
{
    public ServiceDescription Classify(TypeSymbol type)
    {
        var reasons = new List<string>();

        var confidence = 0;

        ServiceKind kind = ServiceKind.Unknown;

        //
        // Nome
        //

        if (type.Name.EndsWith("Controller"))
        {
            kind = ServiceKind.Controller;
            confidence += 80;
            reasons.Add("Type name ends with Controller.");
        }

        else if (type.Name.EndsWith("Service"))
        {
            kind = ServiceKind.Service;
            confidence += 60;
            reasons.Add("Type name ends with Service.");
        }

        else if (type.Name.EndsWith("Repository"))
        {
            kind = ServiceKind.Repository;
            confidence += 80;
            reasons.Add("Type name ends with Repository.");
        }

        else if (type.Name.EndsWith("Factory"))
        {
            kind = ServiceKind.Factory;
            confidence += 80;
            reasons.Add("Type name ends with Factory.");
        }

        else if (type.Name.EndsWith("Handler"))
        {
            kind = ServiceKind.Handler;
            confidence += 70;
            reasons.Add("Type name ends with Handler.");
        }

        else if (type.Name.EndsWith("Validator"))
        {
            kind = ServiceKind.Validator;
            confidence += 70;
            reasons.Add("Type name ends with Validator.");
        }

        else if (type.Name.EndsWith("Mapper"))
        {
            kind = ServiceKind.Mapper;
            confidence += 70;
            reasons.Add("Type name ends with Mapper.");
        }

        //
        // Namespace
        //

        if (type.Namespace.Contains(".Services"))
        {
            confidence += 15;
            reasons.Add("Namespace indicates Services.");
        }

        if (type.Namespace.Contains(".Repositories"))
        {
            confidence += 15;
            reasons.Add("Namespace indicates Repositories.");
        }

        if (type.Namespace.Contains(".Controllers"))
        {
            confidence += 15;
            reasons.Add("Namespace indicates Controllers.");
        }

        //
        // Tipo
        //

        if (!type.Abstract)
        {
            confidence += 5;
            reasons.Add("Concrete type.");
        }

        if (type.Methods.Count > 0)
        {
            confidence += 5;
            reasons.Add("Contains methods.");
        }

        confidence = Math.Min(confidence, 100);

        return new ServiceDescription
        {
            Kind = kind,
            Confidence = confidence,
            Reasons = reasons
        };
    }
}