using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;
using Lamar.IoC.Frames;
using Lamar.IoC.Resolvers;
using Lamar.Scanning.Conventions;
using Lamar.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.IoC.Instances
{
    public class ConstructorInstance<T> : ConstructorInstance
    {
        public ConstructorInstance(Type serviceType, ServiceLifetime lifetime) : base(serviceType, typeof(T), lifetime)
        {
        }
        
        public ConstructorInstance<T> SelectConstructor(Expression<Func<T>> constructor)
        {
            var finder = new ConstructorFinderVisitor(typeof(T));
            finder.Visit(constructor);

            Constructor = finder.Constructor;

            return this;
        }
        
        internal class ConstructorFinderVisitor : ExpressionVisitorBase
        {
            private readonly Type _type;
            private ConstructorInfo _constructor;

            public ConstructorFinderVisitor(Type type)
            {
                _type = type;
            }

            public ConstructorInfo Constructor => _constructor;

            protected override NewExpression VisitNew(NewExpression nex)
            {
                if (nex.Type == _type)
                {
                    _constructor = nex.Constructor;
                }

                return base.VisitNew(nex);
            }
        }
    }

    public class ConstructorInstance : GeneratedInstance, IConfiguredInstance
    {
        public static readonly string NoPublicConstructors = "No public constructors";

        public static readonly string NoPublicConstructorCanBeFilled =
            "Cannot fill the dependencies of any of the public constructors";

        private CtorArg[] _arguments = new CtorArg[0];
        private ObjectInstance _func;


        public ConstructorInstance(Type serviceType, Type implementationType, ServiceLifetime lifetime) : base(
            serviceType, implementationType, lifetime)
        {
            Name = Variable.DefaultArgName(implementationType);
        }

        public ConstructorInfo Constructor { get; set; }



        public static ConstructorInstance For<T>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            return For<T, T>(lifetime);
        }

        public static ConstructorInstance<TConcrete> For<T, TConcrete>(ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TConcrete : T
        {
            return new ConstructorInstance<TConcrete>(typeof(T), lifetime);
        }

        public IList<Instance> InlineDependencies => _inlines;

        public override object QuickResolve(Scope scope)
        {
            if (_resolver != null || Lifetime != ServiceLifetime.Transient) return Resolve(scope);

            var values = _arguments.Select(x => x.Instance.QuickResolve(scope)).ToArray();
            var service = Activator.CreateInstance(ImplementationType, values);

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

            var ctorParameters = _arguments.Select(arg => arg.Resolve(variables, dependencyMode)).ToArray();

            if (_func == null)
            {
                return new ConstructorFrame(this, disposalTracking, ctorParameters).Variable;
            }
            else
            {
                var funcArg = variables.Resolve(_func, BuildMode.Dependency);
                return new CallFuncBuilderFrame(this, disposalTracking, funcArg, ctorParameters).Variable;
            }

            
        }


        public override Frame CreateBuildFrame()
        {
            var variables = new ResolverVariables();
            var ctorParameters = _arguments.Select(arg => arg.Resolve(variables, BuildMode.Dependency)).ToArray();

            if (_func != null)
            {
                var funcArg = variables.Resolve(_func, BuildMode.Dependency);

                return new CallFuncBuilderFrame(this, DisposeTracking.None, funcArg, ctorParameters)
                {
                    ReturnCreated = true
                };
            }
            
            return new ConstructorFrame(this, DisposeTracking.None, ctorParameters)
            {
                ReturnCreated = true
            };
        }



        private DisposeTracking determineDisposalTracking(BuildMode mode)
        {
            if (!ImplementationType.CanBeCastTo<IDisposable>()) return DisposeTracking.None;

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
                _arguments = Constructor.GetParameters()
                    .Select(x => determineArgument(services, x))
                    .Where(x => x.Instance != null).ToArray();


                foreach (var argument in _arguments)
                {
                    argument.Instance.CreatePlan(services);
                }

                if (ImplementationType.MustBeBuiltWithFunc())
                {
                    (var func, var funcType) = CtorFuncBuilder.LambdaTypeFor(ServiceType, ImplementationType, Constructor);
                    _func = new ObjectInstance(funcType, func);



                    services.Inject(_func);
                }
            }


            return _arguments.Select(x => x.Instance);
        }

        

        private CtorArg determineArgument(ServiceGraph services, ParameterInfo x)
        {
            var instance = _inlines.FirstOrDefault(i => i.ServiceType == x.ParameterType && i.Name == x.Name)
                           ?? _inlines.FirstOrDefault(i => i.ServiceType == x.ParameterType)
                           ?? services.FindDefault(x.ParameterType);

            if (instance == null && x.IsOptional && x.DefaultValue == null)
            {
                instance = new NullInstance(x.ParameterType);
            }
            
            return new CtorArg(x, instance);
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
        private IReadOnlyList<Instance> _inlineDependencies;

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

        IReadOnlyList<Instance> IConfiguredInstance.InlineDependencies => _inlineDependencies;
    }
    
    
}
