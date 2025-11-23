using System;
using System.Linq;
using JasperFx.Core;
using JasperFx.Core.Reflection;
using JasperFx.Core.TypeScanning;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.Scanning.Conventions;

internal class FirstInterfaceConvention : IRegistrationConvention
{
    private readonly ServiceLifetime _lifetime;

    public FirstInterfaceConvention(ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        _lifetime = lifetime;
    }

    public void ScanTypes(TypeSet types, ServiceRegistry services)
    {
        foreach (var type in types.FindTypes(TypeClassification.Concretes).Where(x => x.GetConstructors().Any()))
        {
            var interfaceType = type.GetInterfaces().FirstOrDefault(x => x != typeof(IDisposable));
            if (interfaceType != null && !interfaceType.HasAttribute<JasperFxIgnoreAttribute>() &&
                !type.IsOpenGeneric())
            {
                services.AddType(interfaceType, type, _lifetime);
            }
        }
    }

    public override string ToString()
    {
        return "Register all concrete types against the first interface (if any) that they implement";
    }
}