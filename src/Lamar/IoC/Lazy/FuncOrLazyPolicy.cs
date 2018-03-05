using System;
using System.Linq;
using Lamar.IoC.Instances;
using Lamar.Util;

namespace Lamar.IoC.Lazy
{
    public class FuncOrLazyPolicy : IFamilyPolicy
    {
        public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
        {
            if (type.Closes(typeof(Func<>)))
            {
                return new ServiceFamily(type, typeof(FuncInstance<>).CloseAndBuildAs<Instance>(type.GetGenericArguments().Single()));
            }
            
            if (type.Closes(typeof(Lazy<>)))
            {
                return new ServiceFamily(type, typeof(LazyInstance<>).CloseAndBuildAs<Instance>(type.GetGenericArguments().Single()));
            }
            
            if (type.Closes(typeof(Func<,>)) && type.GenericTypeArguments.First() == typeof(string))
            {
                return new ServiceFamily(type, typeof(FuncByNameInstance<>).CloseAndBuildAs<Instance>(type.GetGenericArguments().Last()));
            }

            return null;
        }
    }
}