using System;
using System.Linq;
using Lamar.Codegen;
using Lamar.IoC.Instances;

namespace Lamar
{
    /// <summary>
    /// Custom policy applied at service registration that optionally
    /// wraps the original Instance with a decorator
    /// </summary>
    public interface IDecoratorPolicy
    {
        bool TryWrap(Instance inner, out Instance wrapped);
    }

    public class DecoratorPolicy<TService, TDecorator> : IDecoratorPolicy
        where TDecorator : TService
    {
        public DecoratorPolicy()
        {
            if (typeof(TDecorator).IsAbstract || typeof(TDecorator).IsInterface)
            {
                throw new InvalidOperationException("The decorating type (the 2nd type argument) must be a concrete type");
            }
            
            var hasCtorArg = typeof(TDecorator).GetConstructors().SelectMany(x => x.GetParameters())
                .Any(x => x.ParameterType == typeof(TService));

            if (!hasCtorArg)
            {
                throw new InvalidOperationException($"There must be a constructor argument for the inner {typeof(TService).FullNameInCode()} argument");
            }
        }

        public bool TryWrap(Instance instance, out Instance wrapped)
        {
            
            if (instance.ServiceType == typeof(TService))
            {
                var decorator = new ConstructorInstance(typeof(TService), typeof(TDecorator), instance.Lifetime);
                decorator.AddInline(instance);

                wrapped = decorator;

                return true;
            }
            else
            {
                wrapped = null;
                return false;
            }
        }
    }
}