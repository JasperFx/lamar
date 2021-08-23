using System;
using Lamar.IoC.Instances;

namespace Lamar
{
    internal class EmptyFamilyPolicy : IFamilyPolicy
    {
        public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
        {
            if (!type.IsGenericTypeDefinition) return new ServiceFamily(type, new IDecoratorPolicy[0], new Instance[0]);

            return null;
        }
    }
}