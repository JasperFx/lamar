using System;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
    public abstract class GeneratedInstance : Instance
    {
        private GeneratedType _resolverType;

        protected GeneratedInstance(Type serviceType, Type implementationType, ServiceLifetime lifetime) : base(serviceType, implementationType, lifetime)
        {
        }

        internal (string, string) GenerateResolverClassCode(GeneratedAssembly generatedAssembly)
        {
            var typeName = GetResolverTypeName();


            var buildType = ServiceType.MustBeBuiltWithFunc() || ImplementationType.MustBeBuiltWithFunc()
                ? typeof(object)
                : ServiceType;

            var resolverType = generatedAssembly.AddType(typeName, ResolverBaseType.MakeGenericType(buildType));

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


        public void GenerateResolver(GeneratedAssembly generatedAssembly)
        {
            if (_resolverType != null) return; // got some kind of loop in here we need to short circuit

            if (ErrorMessages.Any() || Dependencies.SelectMany(x => x.ErrorMessages).Any()) return;

            var typeName = (ServiceType.FullNameInCode() + "_" + Name).Sanitize();


            var buildType = ServiceType.MustBeBuiltWithFunc() || ImplementationType.MustBeBuiltWithFunc()
                ? typeof(object)
                : ServiceType;

            _resolverType = generatedAssembly.AddType(typeName, ResolverBaseType.MakeGenericType(buildType));

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

        public abstract Frame CreateBuildFrame();
        protected abstract Variable generateVariableForBuilding(ResolverVariables variables, BuildMode mode, bool isRoot);

        private readonly object _locker = new object();
        protected IResolver _resolver;

        internal IResolver Resolver => _resolver;

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
                        buildResolver(scope);
                    }
                }
            }


            return _resolver.Resolve(scope);
        }

        private void buildResolver(Scope scope)
        {
            if (_resolver != null) return;

            if (ErrorMessages.Any() || Dependencies.Any(x => x.ErrorMessages.Any()))
            {
                _resolver = new ErrorMessageResolver(this);
            }
            else
            {
                if (scope.ServiceGraph.CachedResolverTypes.TryGetValue(GetResolverTypeName(), out var resolverType))
                {
                    _resolver = (IResolver) scope.Root.QuickBuild(resolverType);
                }
                else
                {
                    var assembly = scope.ServiceGraph.ToGeneratedAssembly();
                    GenerateResolver(assembly);

                    if (_resolverType == null)
                    {
                        _resolver = new ErrorMessageResolver(this);
                    }
                    else
                    {
                        assembly.CompileAll();

                        _resolver = (IResolver) scope.Root.QuickBuild(_resolverType.CompiledType);
                        _resolverType.ApplySetterValues(_resolver);
                    }
                }
            }

            _resolver.Hash = GetHashCode();
            _resolver.Name = Name;
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
