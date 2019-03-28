using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lamar.IoC.Frames;
using Lamar.IoC.Resolvers;
using Lamar.Scanning.Conventions;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using Microsoft.Extensions.DependencyInjection;
using LamarCodeGeneration.Util;

namespace Lamar.IoC.Instances
{
    public abstract class GeneratedInstance : Instance
    {
        private GeneratedType _resolverType;

        protected GeneratedInstance(Type serviceType, Type implementationType, ServiceLifetime lifetime) : base(serviceType, implementationType, lifetime)
        {
        }

        internal (string, string) GenerateResolverClassCode(GeneratedAssembly generatedAssembly)
        {
            var typeName = GetResolverTypeName();


            var resolverType = generatedAssembly.AddType(typeName, ResolverBaseType.MakeGenericType(ServiceType));

            var method = resolverType.MethodFor("Build");

            var frame = CreateBuildFrame();

            method.Frames.Add(frame);

            return (typeName, generatedAssembly.GenerateCode());
        }

        public string GetResolverTypeName()
        {
            var typeName = (ServiceType.FullNameInCode() + "_" + Name).Sanitize();
            return typeName;
        }

        protected virtual IEnumerable<Assembly> relatedAssemblies()
        {
            yield return ServiceType.Assembly;
            yield return ImplementationType.Assembly;
        }

        [Obsolete("This will go away when Lamar switches to Expressions")]
        public void GenerateResolver(GeneratedAssembly generatedAssembly)
        {
            if (_resolverType != null) return; // got some kind of loop in here we need to short circuit

            if (ErrorMessages.Any() || Dependencies.SelectMany(x => x.ErrorMessages).Any()) return;

            var typeName = (ServiceType.FullNameInCode() + "_" + Name).Sanitize();


            _resolverType = generatedAssembly.AddType(typeName, ResolverBaseType.MakeGenericType(ServiceType));

            foreach (var relatedAssembly in relatedAssemblies())
            {
                generatedAssembly.ReferenceAssembly(relatedAssembly);
            }

            var method = _resolverType.MethodFor("Build");

            var frame = CreateBuildFrame();

            method.Frames.Add(frame);
        }

        public sealed override Variable CreateVariable(BuildMode mode, ResolverVariables variables, bool isRoot)
        {
            if (Lifetime == ServiceLifetime.Singleton)
            {
                if (mode == BuildMode.Build)
                {
                    return generateVariableForBuilding(variables, mode, isRoot);
                }

                return new InjectedServiceField(this);
            }



            if (Lifetime == ServiceLifetime.Scoped && mode == BuildMode.Dependency)
            {
                return new GetInstanceFrame(this).Variable;
            }



            return generateVariableForBuilding(variables, mode, isRoot);
        }

        public Func<Scope, object> BuildFuncResolver(Scope scope)
        {
            var root = scope.Root;
            var def = new FuncResolverDefinition(this, root);
            var resolver = def.BuildResolver();

            if (Lifetime == ServiceLifetime.Scoped)
            {
                var locker = new object();
                
                return s =>
                {
                    if (s.Services.TryGetValue(Hash, out object service))
                    {
                        return service;
                    }

                    lock (locker)
                    {
                        if (s.Services.TryGetValue(Hash, out service))
                        {
                            return service;
                        }

                        service = resolver(s);
                        s.Services.Add(Hash, service);

                        return service;
                    }
                    

                };
            }
            
            if (Lifetime == ServiceLifetime.Singleton)
            {
                var locker = new object();
                
                return s =>
                {
                    if (root.Services.TryGetValue(Hash, out object service))
                    {
                        return service;
                    }

                    lock (locker)
                    {
                        if (root.Services.TryGetValue(Hash, out service))
                        {
                            return service;
                        }

                        service = resolver(root);
                        root.Services.Add(Hash, service);

                        return service;
                    }
                    

                };
            }


            return resolver;
        }

        public abstract Frame CreateBuildFrame();
        protected abstract Variable generateVariableForBuilding(ResolverVariables variables, BuildMode mode, bool isRoot);

        private readonly object _locker = new object();
        protected Func<Scope, object> _resolver;


        public override Func<Scope, object> ToResolver(Scope topScope)
        {
            if (_resolver == null)
            {
                lock (_locker)
                {
                    if (_resolver == null)
                    {
                        buildResolver(topScope);
                    }
                }
            }

            return _resolver;
        }

        public override object Resolve(Scope scope)
        {
            if (_resolver != null) return _resolver(scope);
            
            lock (_locker)
            {
                if (_resolver == null)
                {
                    buildResolver(scope);
                }
            }


            return _resolver(scope);
        }

        private void buildResolver(Scope scope)
        {
            if (_resolver != null) return;

            if (ErrorMessages.Any() || Dependencies.Any(x => x.ErrorMessages.Any()))
            {
                var errorResolver = new ErrorMessageResolver(this);
                _resolver = errorResolver.Resolve;
                
            }
            else
            {
                _resolver = BuildFuncResolver(scope);
            }

        }

        internal override string GetBuildPlan(Scope rootScope)
        {
            if (_resolverType == null)
            {
                var (name, code) = GenerateResolverClassCode(rootScope.ServiceGraph.ToGeneratedAssembly());
                return code;
            }

            if (_resolverType != null)
            {
                return _resolverType.SourceCode;
            }

            if (ErrorMessages.Any())
            {
                return "Errors!" + Environment.NewLine + ErrorMessages.Join(Environment.NewLine);
            }

            return ToString();
        }

        public Type ResolverBaseType
        {
            get
            {
                switch (Lifetime)
                {
                    case ServiceLifetime.Scoped:
                        return typeof(ScopedResolver<>);

                    case ServiceLifetime.Singleton:
                        return typeof(SingletonResolver<>);

                    case ServiceLifetime.Transient:
                        return typeof(TransientResolver<>);
                }

                return null;
            }
        }
    }

}
