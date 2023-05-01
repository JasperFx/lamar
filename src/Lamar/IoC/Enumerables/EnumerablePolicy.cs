using System;
using System.Linq;
using JasperFx.Core.Reflection;
using Lamar.IoC.Instances;

namespace Lamar.IoC.Enumerables;

#region sample_EnumerablePolicy

internal class EnumerablePolicy : IFamilyPolicy
{
    public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
    {
        if (type.IsArray)
        {
            var instanceType = typeof(ArrayInstance<>).MakeGenericType(type.GetElementType());
            var instance = Activator.CreateInstance(instanceType, type).As<Instance>();
            return new ServiceFamily(type, new IDecoratorPolicy[0], instance);
        }

        if (type.IsEnumerable())
        {
            var elementType = type.GetGenericArguments().First();

            var instanceType = typeof(ListInstance<>).MakeGenericType(elementType);
            var ctor = instanceType.GetConstructors().Single();
            var instance = ctor.Invoke(new object[] { type }).As<Instance>();

            return new ServiceFamily(type, new IDecoratorPolicy[0], instance);
        }

        return null;
    }
}

#endregion