using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lamar.IoC.Activation;
using Lamar.IoC.Frames;
using Lamar.IoC.Setters;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using LamarCodeGeneration.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Instances
{
    /// <summary>
    /// Models the construction of a service registration that is built by calling the implementation
    /// type's constructor functions
    /// </summary>
    /// <typeparam name="TImplementation"></typeparam>
    /// <typeparam name="TService"></typeparam>
    public partial class ConstructorInstance<TImplementation, TService> : ConstructorInstance, IMaybeIntercepted where TImplementation : TService
    {
        private Func<IServiceContext,TImplementation, TService> _interceptor;

        public ConstructorInstance(Type serviceType, ServiceLifetime lifetime) : base(serviceType, typeof(TImplementation), lifetime)
        {
        }
        
        public ConstructorInstance<TImplementation, TService> SelectConstructor(Expression<Func<TImplementation>> constructor)
        {
            var finder = new ConstructorFinderVisitor<TImplementation>(typeof(TImplementation));
            finder.Visit(constructor);

            Constructor = finder.Constructor;

            return this;
        }
        
        /// <summary>
        /// Intercept the object being created and potentially replace it with a wrapped
        /// version or another object
        /// </summary>
        /// <param name="interceptor"></param>
        /// <returns></returns>
        public ConstructorInstance<TImplementation, TService> OnCreation(Func<TImplementation, TService> interceptor)
        {
            return OnCreation((s, x) => interceptor(x));
        }
        
        /// <summary>
        /// Intercept the object being created and potentially replace it with a wrapped
        /// version or another object
        /// </summary>
        /// <param name="interceptor"></param>
        /// <returns></returns>
        public ConstructorInstance<TImplementation, TService> OnCreation(Func<IServiceContext, TImplementation, TService> interceptor)
        {
            _interceptor = interceptor;
            return this;
        }
        
        /// <summary>
        /// Perform some action on the object being created at the time the object is created for the first time by Lamar
        /// </summary>
        /// <param name="activator"></param>
        /// <returns></returns>
        public ConstructorInstance<TImplementation, TService> OnCreation(Action<IServiceContext, TImplementation> activator)
        {
            return OnCreation((s, x) =>
            {
                activator(s, x);
                return x;
            });
        }
        
        /// <summary>
        /// Perform some action on the object being created at the time the object is created for the first time by Lamar
        /// </summary>
        /// <param name="activator"></param>
        /// <returns></returns>
        public ConstructorInstance<TImplementation, TService> OnCreation(Action<TImplementation> activator)
        {
            return OnCreation((s, x) =>
            {
                activator(x);
                return x;
            });
        }

        bool IMaybeIntercepted.TryWrap(out Instance wrapped)
        {
            if (_interceptor != null)
            {
                wrapped = new InterceptingInstance<TImplementation, TService>(_interceptor, this);
                return true;
            }

            wrapped = null;
            return false;
        }
    }

    public class ConstructorInstance : GeneratedInstance, IConfiguredInstance
    {
        public static readonly string NoPublicConstructors = "No public constructors";

        public static readonly string NoPublicConstructorCanBeFilled =
            "Cannot fill the dependencies of any of the public constructors";



        public ConstructorInstance(Type serviceType, Type implementationType, ServiceLifetime lifetime) : base(
            serviceType, implementationType, lifetime)
        {
            Name = Variable.DefaultArgName(implementationType);
        }

        public ConstructorInfo Constructor { get; set; }

        public CtorArg[] Arguments { get; private set; } = new CtorArg[0];

        public static ConstructorInstance For<T>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            return For<T, T>(lifetime);
        }

        public static ConstructorInstance<TConcrete, T> For<T, TConcrete>(ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TConcrete : T
        {
            return new(typeof(T), lifetime);
        }

        public IList<Instance> InlineDependencies => _inlines;

        public override Func<Scope, object> ToResolver(Scope topScope)
        {
            if (Lifetime == ServiceLifetime.Singleton)
            {
                return s =>
                {
                    if (topScope.Services.TryFind(Hash, out object service))
                    {
                        return service;
                    }

                    lock (_locker)
                    {
                        service = ((Func<Scope, object>) quickResolve)(topScope);
                    }

                    return service;
                };
            }
            
            return base.ToResolver(topScope);
        }
        

        private readonly object _locker = new object();
        public override object QuickResolve(Scope scope)
        {
            if (_resolver != null) return _resolver(scope);

            if (Lifetime == ServiceLifetime.Singleton)
            {
                lock (_locker)
                {
                    return quickResolve(scope);
                }
            }

            return quickResolve(scope);
        }

        private object quickResolve(Scope scope)
        {
            var holdingScope = Lifetime == ServiceLifetime.Singleton ? scope.Root : scope;
            if (tryGetService(holdingScope, out var cached))
            {
                return cached;
            }

            if (Constructor == null && ErrorMessages.Any())
            {
                new ErrorMessageResolver(this).Resolve(scope);
            }
            
            var values = Arguments.Select(x => x.Instance.QuickResolve(holdingScope)).ToArray();
            var service = Activator.CreateInstance(ImplementationType, values);

            foreach (var setter in _setters)
            {
                setter.ApplyQuickBuildProperties(service, scope);
            }

            switch (service)
            {
                case IDisposable disposable when Lifetime == ServiceLifetime.Singleton:
                    scope.Root.Disposables.Add(disposable);
                    break;
                case IDisposable disposable:
                    scope.Disposables.Add(disposable);
                    break;
                case IAsyncDisposable a:
                {
                    var wrapper = new AsyncDisposableWrapper(a);
                    if (Lifetime == ServiceLifetime.Singleton)
                    {
                        scope.Root.Disposables.Add(wrapper);
                    }
                    else
                    {
                        scope.Disposables.Add(wrapper);
                    }

                    break;
                }
            }

            if (Lifetime != ServiceLifetime.Transient)
            {
                store(holdingScope, service);
            }


            return service;
        }

        public override Instance CloseType(Type serviceType, Type[] templateTypes)
        {
            if (!ImplementationType.IsOpenGeneric())
                return null;

            Type closedType;
            try
            {
                closedType = ImplementationType.MakeGenericType(templateTypes);
            }
            catch
            {
                return null;
            }

            var closedInstance = new ConstructorInstance(serviceType, closedType, Lifetime);
            foreach (var instance in InlineDependencies)
            {
                if (instance.ServiceType.IsOpenGeneric())
                {
                    var closed = instance.CloseType(instance.ServiceType.MakeGenericType(templateTypes), templateTypes);
                    closedInstance.AddInline(closed);
                }
                else
                {
                    closedInstance.AddInline(instance);
                }
            }

            return closedInstance;
        }


        protected override Variable generateVariableForBuilding(ResolverVariables variables, BuildMode mode, bool isRoot)
        {
            var disposalTracking = determineDisposalTracking(mode);

            // This is goofy, but if the current service is the top level root of the resolver
            // being created here, make the dependencies all be Dependency mode
            var dependencyMode = isRoot && mode == BuildMode.Build ? BuildMode.Dependency : mode;

            var ctorParameters = Arguments.Select(arg => arg.Resolve(variables, dependencyMode)).ToArray();
            var setterParameters = _setters.Select(arg => arg.Resolve(variables, dependencyMode)).ToArray();

            
            return new InstanceConstructorFrame(this, disposalTracking, ctorParameters, setterParameters).Variable;
        }


        public override Frame CreateBuildFrame()
        {
            var variables = new ResolverVariables();
            var ctorParameters = Arguments.Select(arg => arg.Resolve(variables, BuildMode.Dependency)).ToArray();

            var setterParameters = _setters.Select(arg => arg.Resolve(variables, BuildMode.Dependency)).ToArray();
            
            variables.MakeNamesUnique();

            return new InstanceConstructorFrame(this, DisposeTracking.None, ctorParameters, setterParameters)
            {
                Mode = ConstructorCallMode.ReturnValue
            };
        }

        private DisposeTracking determineDisposalTracking(BuildMode mode)
        {
            if (!ImplementationType.CanBeCastTo<IDisposable>() && !ImplementationType.CanBeCastTo<IAsyncDisposable>()) return DisposeTracking.None;

            switch (mode)
            {
                case BuildMode.Inline:
                    return DisposeTracking.WithUsing;

                case BuildMode.Dependency:
                    return DisposeTracking.RegisterWithScope;

                case BuildMode.Build:
                    return DisposeTracking.None;
            }

            return DisposeTracking.None;
        }



        protected override IEnumerable<Instance> createPlan(ServiceGraph services)
        {
            Constructor = DetermineConstructor(services, out var message);

            if (message.IsNotEmpty()) ErrorMessages.Add(message);

            if (Constructor != null)
            {
                buildOutConstructorArguments(services);
                findSetters(services);
            }

            return Arguments.Select(x => x.Instance).Concat(_setters.Select(x => x.Instance));
        }
        
        private readonly List<InjectedSetter> _setters = new List<InjectedSetter>();

        internal IReadOnlyList<InjectedSetter> Setters => _setters;

        internal InjectedSetter[] FindSetters(ServiceGraph services)
        {
            findSetters(services);
            return _setters.ToArray();
        }

        private void findSetters(ServiceGraph services)
        {
            foreach (var property in ImplementationType.GetProperties().Where(x => x.CanWrite && x.SetMethod.IsPublic))
            {
                var instance = findInlineDependency(property.Name, property.PropertyType);
                if (instance == null && services.ShouldBeSet(property))
                {
                    instance = services.FindDefault(property.PropertyType);
                }

                if (instance != null) _setters.Add(new InjectedSetter(property, instance));
            }
            
            foreach (var setter in _setters)
            {
                setter.Instance.CreatePlan(services);
            }
        }

        private void buildOutConstructorArguments(ServiceGraph services)
        {
            Arguments = Constructor.GetParameters()
                .Select(x => determineArgument(services, x))
                .Where(x => x.Instance != null).ToArray();


            foreach (var argument in Arguments)
            {
                argument.Instance.CreatePlan(services);
            }
        }


        private CtorArg determineArgument(ServiceGraph services, ParameterInfo parameter)
        {
            var dependencyType = parameter.ParameterType;
            var instance = findInstanceForConstructorParameter(services, parameter, dependencyType);

            return new CtorArg(parameter, instance);
        }

        private Instance findInstanceForConstructorParameter(ServiceGraph services, ParameterInfo parameter, Type dependencyType)
        {
            var instance = findInlineDependency(parameter.Name, dependencyType);
            if (instance != null) return instance;

            if (parameter.IsOptional)
            {
                if (parameter.DefaultValue == null)
                {
                    return services.FindInstance(parameter) ?? new NullInstance(dependencyType);
                }

                return new ObjectInstance(parameter.ParameterType, parameter.DefaultValue);
            }

            return services.FindInstance(parameter);

        }

        private Instance findInlineDependency(string name, Type dependencyType)
        {
            var exact = _inlines.FirstOrDefault(i => i.ServiceType == dependencyType && i.Name == name);
            if (exact != null) return exact;

            var instance = _inlines.FirstOrDefault(i => i.ServiceType == dependencyType);
            if (instance == null) return null;
            return instance.InlineIsLimitedToExactNameMatch ? null : instance;
        }


        public override string ToString()
        {
            string text = $"new {ImplementationType.ShortNameInCode()}()";
            
            if (Constructor != null)
            {
                text = $"new {ImplementationType.ShortNameInCode()}({Constructor.GetParameters().Select(x => x.Name).Join(", ")})";
            }

            return text;
        }

        private static ConstructorInfo[] findConstructors(Type implementationType)
        {
            var publics = implementationType.GetConstructors() ?? new ConstructorInfo[0];

            if (publics.Any()) return publics;
            
            
            
            if (implementationType.IsPublic)
            {
                return new ConstructorInfo[0];
            }


            return implementationType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance |
                                                      BindingFlags.Public) ?? new ConstructorInfo[0];

        }

        private bool couldBuild(ConstructorInfo ctor, ServiceGraph services)
        {
            return ctor.GetParameters().All(p =>
                services.FindDefault(p.ParameterType) != null || _inlines.Any(x => x.ServiceType == p.ParameterType) ||
                p.IsOptional);
        }
        
        public ConstructorInfo DetermineConstructor(ServiceGraph services,
            out string message)
        {
            message = null;

            if (Constructor != null) return Constructor;

            var fromAttribute = DefaultConstructorAttribute.GetConstructor(ImplementationType);
            if (fromAttribute != null) return fromAttribute;

            var constructors = findConstructors(ImplementationType);


            if (constructors.Any())
            {
                var ctor = constructors
                    .OrderByDescending(x => x.GetParameters().Length)
                    .FirstOrDefault(x => couldBuild(x, services));

                if (ctor == null)
                {
                    message = NoPublicConstructorCanBeFilled;
                    message += $"{Environment.NewLine}Available constructors:";

                    foreach (var constructor in constructors)
                    {
                        message += explainWhyConstructorCannotBeUsed(ImplementationType, constructor, services);
                        message += Environment.NewLine;
                    }

                }

                return ctor;
            }

            message = NoPublicConstructors;

            return null;
        }

        private static string explainWhyConstructorCannotBeUsed(Type implementationType, ConstructorInfo constructor,
            ServiceGraph services)
        {

            var args = constructor.GetParameters().Select(x => $"{x.ParameterType.NameInCode()} {x.Name}").Join(", ");
            var declaration = $"new {implementationType.NameInCode()}({args})";

            foreach (var parameter in constructor.GetParameters())
            {
                // TODO -- this will change with inline dependencies
                if (parameter.ParameterType.IsSimple())
                {
                    declaration +=
                        $"{Environment.NewLine}* {parameter.ParameterType.NameInCode()} {parameter.Name} is a 'simple' type that cannot be auto-filled";
                }
                else
                {
                    var @default = services.FindDefault(parameter.ParameterType);
                    if (@default == null)
                    {
                        declaration +=
                            $"{Environment.NewLine}* {parameter.ParameterType.NameInCode()} is not registered within this container and cannot be auto discovered by any missing family policy";
                    }
                }
            }

            

            return declaration;
        }
        
        private readonly IList<Instance> _inlines = new List<Instance>();

        /// <summary>
        /// Adds an inline dependency
        /// </summary>
        /// <param name="instance"></param>
        public void AddInline(Instance instance)
        {
            instance.Parent = this;
            _inlines.Add(instance);
        }
        
        
        /// <summary>
        ///     Inline definition of a constructor dependency.  Select the constructor argument by type and constructor name.
        ///     Use this method if there is more than one constructor arguments of the same type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="constructorArg"></param>
        /// <returns></returns>
        public DependencyExpression<T> Ctor<T>(string constructorArg = null)
        {
            return new DependencyExpression<T>(this, constructorArg);
        }
        
        /// <summary>
        ///     Inline definition of a setter dependency.  Select the setter property by type and optionally by property name.
        ///     Use this method if there is more than one constructor arguments of the same type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propName"></param>
        /// <returns></returns>
        public DependencyExpression<T> Setter<T>(string propName = null)
        {
            return new DependencyExpression<T>(this, propName);
        }

        IReadOnlyList<Instance> IConfiguredInstance.InlineDependencies { get; }


        protected override IEnumerable<Assembly> relatedAssemblies()
        {
            return base.relatedAssemblies().Concat(_inlines.SelectMany(x => x.ReferencedAssemblies()));
        }
    }
    
    
}
