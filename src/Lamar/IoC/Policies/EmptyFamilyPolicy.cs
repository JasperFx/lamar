using System;

namespace Lamar;

internal class EmptyFamilyPolicy : IFamilyPolicy
{
    public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
    {
        if (!type.IsGenericTypeDefinition)
        {
            return new ServiceFamily(type, new IDecoratorPolicy[0]);
        }

        return null;
    }
}