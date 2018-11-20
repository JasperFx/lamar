using System;
using System.Collections.Generic;
using System.Linq;
using Lamar.IoC;
using Lamar.IoC.Exports;
using Lamar.IoC.Instances;
using Lamar.Scanning.Conventions;
using LamarCompiler.Util;

namespace Lamar
{
    internal class QueryModel : IModel
    {
        private readonly Scope _scope;

        public QueryModel(Scope scope)
        {
            _scope = scope;
        }

        public IServiceFamilyConfiguration For<T>()
        {
            return For(typeof(T));
        }

        public IServiceFamilyConfiguration For(Type type)
        {
            return new ServiceFamilyConfiguration(_scope.ServiceGraph.ResolveFamily(type), _scope);
        }

        public IEnumerable<IServiceFamilyConfiguration> ServiceTypes =>
            _scope.ServiceGraph.Families.Values.Select(x => new ServiceFamilyConfiguration(x, _scope));
        
        public IEnumerable<InstanceRef> InstancesOf(Type serviceType)
        {
            return For(serviceType).Instances;
        }

        public IEnumerable<InstanceRef> InstancesOf<T>()
        {
            return InstancesOf(typeof(T));
        }

        public Type DefaultTypeFor<T>()
        {
            return DefaultTypeFor(typeof(T));
        }

        public Type DefaultTypeFor(Type serviceType)
        {
            return _scope.ServiceGraph.FindDefault(serviceType)?.ImplementationType;
        }

        public IEnumerable<InstanceRef> AllInstances => _scope.ServiceGraph.AllInstances().Select(x => new InstanceRef(x, _scope)).ToArray();
        public T[] GetAllPossible<T>() where T : class
        {
            return AllInstances.ToArray()
                .Where(x => x.ImplementationType.CanBeCastTo(typeof(T)))
                .Select(x => x.Resolve())
                .OfType<T>()
                .ToArray();
        }

        public bool HasRegistrationFor(Type serviceType)
        {
            return _scope.ServiceGraph.FindDefault(serviceType) != null;
        }

        public bool HasRegistrationFor<T>()
        {
            return _scope.ServiceGraph.FindDefault(typeof(T)) != null;
        }

        public IEnumerable<AssemblyScanner> Scanners => _scope.ServiceGraph.Scanners;

        public void ExportResolverCode<T>(string path) where T : CachedResolverSet, new()
        {
            ExportResolverCode(new T(), path);
        }

        public void ExportResolverCode(CachedResolverSet resolverSet, string path)
        {
            resolverSet.Export(_scope.ServiceGraph, AllInstances.Select(x => x.Instance).OfType<GeneratedInstance>().ToArray(), path);
        }

    }
}