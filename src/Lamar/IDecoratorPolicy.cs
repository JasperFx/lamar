using System;
using System.Linq;
using Lamar.Codegen;
using Lamar.IoC.Instances;
using Lamar.Util;

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

    [LamarIgnore]
    public class DecoratorPolicy<TService, TDecorator> : DecoratorPolicy where TDecorator : TService
    {
        public DecoratorPolicy() : base(typeof(TService), typeof(TDecorator))
        {
        }
    }

    [LamarIgnore]
    public class DecoratorPolicy : IDecoratorPolicy
    {
        private readonly Type _serviceType;
        private readonly Type _decoratorType;

        public DecoratorPolicy(Type serviceType, Type decoratorType)
        {
            if (decoratorType.IsAbstract || decoratorType.IsInterface)
            {
                throw new InvalidOperationException("The decorating type (the 2nd type argument) must be a concrete type");
            }

            if (serviceType.IsOpenGeneric())
            {
                if (!decoratorType.IsOpenGeneric() || !GenericsPluginGraph.CanBeCast(serviceType, decoratorType))
                {
                    throw new InvalidOperationException($"{decoratorType.FullNameInCode()} cannot be cast to {serviceType.FullNameInCode()}");
                }
            }

            _serviceType = serviceType;
            _decoratorType = decoratorType;

            if (!serviceType.IsOpenGeneric())
            {
                var hasCtorArg = decoratorType.GetConstructors().SelectMany(x => x.GetParameters())
                    .Any(x =>
                    {
                        return x.ParameterType == serviceType;
                    });

                if (!hasCtorArg)
                {
                    throw new InvalidOperationException($"There must be a constructor argument for the inner {serviceType.FullNameInCode()} argument");
                }
            }
        }

        public bool TryWrap(Instance instance, out Instance wrapped)
        {
            if (_serviceType.IsOpenGeneric())
            {
                if (instance.ServiceType.Closes(_serviceType))
                {
                    var args = instance.ServiceType.GetGenericArguments();
                    var concreteType = _decoratorType.MakeGenericType(args);
                
                    var decorator = new ConstructorInstance(instance.ServiceType, concreteType, instance.Lifetime);
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
            else
            {
                if (instance.ServiceType == _serviceType)
                {
                    var decorator = new ConstructorInstance(_serviceType, _decoratorType, instance.Lifetime);
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
}