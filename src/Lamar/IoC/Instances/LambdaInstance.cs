using System;
using Lamar.IoC.Frames;
using Lamar.IoC.Resolvers;
using LamarCodeGeneration;
using LamarCodeGeneration.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Instances
{
    public class LambdaInstance : LambdaInstance<IServiceProvider, object>
    {
        public LambdaInstance(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime) : base(serviceType, factory, lifetime)
        {
        }

        public static LambdaInstance<TContainer, TReturn> For<TContainer, TReturn>(Func<TContainer, TReturn> func)
        {
            return new LambdaInstance<TContainer, TReturn>(typeof(TReturn), func, ServiceLifetime.Transient);
        }
        
        public static LambdaInstance For<T>(Func<IServiceProvider, T> factory,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            return new LambdaInstance(typeof(T), s => factory(s), lifetime);
        }
    }
    
    public class LambdaInstance<TContainer, TReturn> : Instance
    {
        public Func<TContainer, TReturn> Factory { get; }

        public LambdaInstance(Type serviceType, Func<TContainer, TReturn> factory, ServiceLifetime lifetime) :
            base(serviceType, serviceType, lifetime)
        {
            Factory = factory;
            Name = serviceType.NameInCode();
        }


        // This is important. If the lambda instance is a singleton, it's injected as a singleton
        // to an object constructor and does not need the ServiceProvider
        public override bool RequiresServiceProvider => Lifetime != ServiceLifetime.Singleton;
        public string Description { get; set; }

        public override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            if (Lifetime == ServiceLifetime.Singleton && mode != BuildMode.Build)
            {
                return new InjectedServiceField(this);
            }

            if (Lifetime == ServiceLifetime.Transient && mode != BuildMode.Build)
            {
                return CreateInlineVariable(variables);
            }

            return new GetInstanceFrame(this).Variable;
        }

        public override Variable CreateInlineVariable(ResolverVariables variables)
        {
            var setter = new Setter(typeof(Func<TContainer, TReturn>), inlineSetterName())
            {
                InitialValue = Factory
            };

            return new InlineLambdaCreationFrame<TContainer>(setter, this).Variable;
        }

        private IResolver _resolver;
        private readonly object _locker = new object();

        public override Func<Scope, object> ToResolver(Scope topScope)
        {
            
            
            if (_resolver == null)
            {
                lock (_locker)
                {
                    if (_resolver == null)
                    {
                        _resolver = buildResolver(topScope.Root);
                        _resolver.Hash = Hash;
                        _resolver.Name = Name;
                    }
                }
            }

            return scope => _resolver.Resolve(scope);
        }

        public override object Resolve(Scope scope)
        {
            if (_resolver == null)
            {
                lock (_locker)
                {
                    if (_resolver == null)
                    {
                        _resolver = buildResolver(scope.Root);
                        _resolver.Hash = Hash;
                        _resolver.Name = Name;
                    }
                }
            }

            return _resolver.Resolve(scope);
        }

        protected IResolver buildResolver(Scope rootScope)
        {
            switch (Lifetime)
            {
                case ServiceLifetime.Transient:
                    return new TransientLambdaResolver<TContainer, TReturn>(Factory);

                case ServiceLifetime.Scoped:
                    return new ScopedLambdaResolver<TContainer, TReturn>(Factory);
                    
                case ServiceLifetime.Singleton:
                    return new SingletonLambdaResolver<TContainer, TReturn>(Factory, rootScope);
            }

            throw new ArgumentOutOfRangeException(nameof(Lifetime));
        }
        
        

        public override string ToString()
        {
            return Description ?? $"Lambda Factory of {ServiceType.NameInCode()}";
        }
    }
}
