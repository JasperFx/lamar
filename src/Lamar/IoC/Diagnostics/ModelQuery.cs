using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JasperFx.Core;
using JasperFx.Core.Reflection;
using Lamar.Diagnostics;

namespace Lamar.IoC.Diagnostics;

public class ModelQuery
{
    /// <summary>
    ///     Only list out registrations for service types from this assembly
    /// </summary>
    public Assembly Assembly;

    /// <summary>
    ///     Optionally specify the namespace of service types in the query. This is inclusive.
    /// </summary>
    public string Namespace;

    /// <summary>
    ///     Only list out registrations for the specific ServiceType
    /// </summary>
    public Type ServiceType;


    public string TypeName;

    public IEnumerable<IServiceFamilyConfiguration> Query(IModel model)
    {
        var enumerable = model.ServiceTypes;

        if (Namespace.IsNotEmpty())
        {
            enumerable = enumerable.Where(x => x.ServiceType.IsInNamespace(Namespace));
        }

        if (ServiceType != null)
        {
            enumerable = enumerable.Where(x => x.ServiceType == ServiceType);
        }

        if (Assembly != null)
        {
            enumerable = enumerable.Where(x => x.ServiceType.GetTypeInfo().Assembly == Assembly);
        }

        if (TypeName.IsNotEmpty())
        {
            enumerable = enumerable.Where(x => x.ServiceType.Name.ToLower().Contains(TypeName.ToLower()));
        }

        return enumerable;
    }
}