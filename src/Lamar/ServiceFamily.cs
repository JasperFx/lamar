using System;
using System.Collections.Generic;
using System.Linq;
using Lamar.Codegen;
using Lamar.IoC.Instances;
using Lamar.IoC.Resolvers;
using Lamar.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar
{
    public class ServiceFamily : IServiceFamilyConfiguration
    {
        private readonly Dictionary<string, Instance> _instances = new Dictionary<string, Instance>();


        public Type ServiceType { get; }

        public ServiceFamily(Type serviceType, params Instance[] instances)
        {
            foreach (var instance in instances)
            {
                instance.IsDefault = false;
                instance.IsOnlyOneOfServiceType = false;
            }

            if (instances.Any())
            {
                instances.Last().IsDefault = true;
            }

            if (instances.Length == 1)
            {
                instances[0].IsOnlyOneOfServiceType = true;
            }

            ServiceType = serviceType;

            Default = instances.LastOrDefault();


            makeNamesUnique(instances);

            foreach (var instance in instances)
            {
                _instances.Add(instance.Name, instance);
            }

            All = instances;

            FullNameInCode = serviceType.FullNameInCode();
        }

        public string FullNameInCode { get; }


        public AppendState Append(ObjectInstance instance)
        {
            return Append(new Instance[]{instance});
        }

        public AppendState Append(IEnumerable<ServiceDescriptor> services)
        {
            var instances = services.Select(Instance.For).ToArray();


            return Append(instances);
        }

        public AppendState Append(Instance[] instances)
        {
            var currentDefault = Default;
            
            foreach (var instance in instances)
            {
                instance.IsDefault = false;
            }

            if (instances.Any())
            {
                instances.Last().IsDefault = true;
            }

            Default = instances.LastOrDefault();


            var all = All.Concat(instances).ToArray();
            makeNamesUnique(all);

            _instances.Clear();
            
            foreach (var instance in all)
            {
                while (_instances.ContainsKey(instance.Name))
                {
                    instance.Name += "_2";
                }   
                
                _instances.Add(instance.Name, instance);
            }

            foreach (var instance in all)
            {
                instance.IsOnlyOneOfServiceType = false;
            }

            if (all.Length == 1)
            {
                all[0].IsOnlyOneOfServiceType = true;
            }

            All = all;

            if (currentDefault == null && Default != null) return AppendState.NewDefault;

            return currentDefault == Default ? AppendState.SameDefault : AppendState.NewDefault;
        }

        public override string ToString()
        {
            return $"{nameof(ServiceType)}: {ServiceType.FullNameInCode()}";
        }

        // Has to be in order here
        public Instance[] All { get; private set; }

        public Instance InstanceFor(string name)
        {
            return Instances.ContainsKey(name) ? Instances[name] : null;
        }

        private void makeNamesUnique(IEnumerable<Instance> instances)
        {
            instances
                .GroupBy(x => x.Name)
                .Select(x => x.ToArray())
                .Where(x => x.Length > 1)
                .Each(array =>
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i].Name += (i + 1).ToString();
                    }
                });
        }

        public Instance Default { get; private set; }

        IEnumerable<Instance> IServiceFamilyConfiguration.Instances => _instances.Values;

        bool IServiceFamilyConfiguration.HasImplementations()
        {
            return _instances.Any();
        }

        public IReadOnlyDictionary<string, Instance> Instances => _instances;


        /// <summary>
        /// If the ServiceType is an open generic type, this method will create a
        /// closed type copy of this PluginFamily
        /// </summary>
        /// <param name="types"></param>
        /// <param name="templateTypes"></param>
        /// <returns></returns>
        public ServiceFamily CreateTemplatedClone(Type serviceType, Type[] templateTypes)
        {
            if (!ServiceType.IsGenericType) throw new InvalidOperationException($"{ServiceType.FullNameInCode()} is not an open generic type");

            var instances = _instances.Values.ToArray().Select(x => {
                var clone = x.CloseType(serviceType, templateTypes);
                if (clone == null) return null;

                clone.Name = x.Name;
                return clone;
            }).Where(x => x != null).ToArray();

            return new ServiceFamily(serviceType, instances);
        }


    }

    public enum AppendState
    {
        NewDefault,
        SameDefault
    }
}
