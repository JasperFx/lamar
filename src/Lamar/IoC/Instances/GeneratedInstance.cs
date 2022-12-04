﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JasperFx.Core;
using Lamar.IoC.Frames;
using Lamar.IoC.Resolvers;
using Lamar.Util;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;
using Microsoft.Extensions.DependencyInjection;
using JasperFx.CodeGeneration.Util;

namespace Lamar.IoC.Instances
{
    public abstract class GeneratedInstance : Instance
    {
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

            switch (Lifetime)
            {
                case ServiceLifetime.Scoped:
                {
                    var locker = new object();
                
                    return s =>
                    {
                        if (s.Services.TryFind(Hash, out object service))
                        {
                            return service;
                        }

                        lock (locker)
                        {
                            if (s.Services.TryFind(Hash, out service))
                            {
                                return service;
                            }

                            service = resolver(s);

                            s.Services = s.Services.AddOrUpdate(Hash, service);

                            return service;
                        }
                    

                    };
                }
                
                case ServiceLifetime.Singleton:
                {
                    var locker = new object();
                
                    return s =>
                    {
                        if (root.Services.TryFind(Hash, out object service))
                        {
                            return service;
                        }

                        lock (locker)
                        {
                            if (root.Services.TryFind(Hash, out service))
                            {
                                return service;
                            }

                            service = resolver(root);
                            root.Services = root.Services.AddOrUpdate(Hash, service);

                            return service;
                        }
                    

                    };
                }
                default:
                    return resolver;
            }
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

            lock (_locker)
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



        }

        internal override string GetBuildPlan(Scope rootScope)
        {
            if (ErrorMessages.Any())
            {
                return "Errors!" + Environment.NewLine + ErrorMessages.Join(Environment.NewLine);
            }
            
            var (name, code) = GenerateResolverClassCode(rootScope.ServiceGraph.ToGeneratedAssembly());
            return code;

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
