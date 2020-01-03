using System;
using System.Collections.Generic;
using System.Linq;
using Lamar.IoC.Instances;
using LamarCodeGeneration;
using Microsoft.Extensions.DependencyInjection;
using LamarCodeGeneration.Util;

namespace Lamar
{
    public class ServiceFamily 
    {
        private readonly Dictionary<string, Instance> _instances = new Dictionary<string, Instance>();


        public Type ServiceType { get; }

        public ServiceFamily(Type serviceType, IDecoratorPolicy[] decoratorPolicies, params Instance[] instances)
        {
            if (!serviceType.IsOpenGeneric())
            {
                // First pass, see if you need to apply any decorators first
                instances = applyDecorators(decoratorPolicies, instances).ToArray();
            }
            
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

        private IEnumerable<Instance> applyDecorators(IDecoratorPolicy[] decoratorPolicies, Instance[] instances)
        {
            foreach (var instance in instances)
            {
                Instance current = instance;

                var originalName = instance.Name;
                var originalLifetime = instance.Lifetime;
                
                
                foreach (var decoratorPolicy in decoratorPolicies)
                {
                    if (decoratorPolicy.TryWrap(current, out var wrapped))
                    {
                        wrapped.Lifetime = originalLifetime;
                        wrapped.Name = originalName;

                        current.Lifetime = ServiceLifetime.Transient;
                        
                        current = wrapped;
                        
                    }
                }

                yield return current;
            }
        }

        public string FullNameInCode { get; }


        public AppendState Append(ObjectInstance instance, IDecoratorPolicy[] decoration)
        {
            return Append(new Instance[]{instance}, decoration);
        }

        public AppendState Append(IEnumerable<ServiceDescriptor> services, IDecoratorPolicy[] decoration)
        {
            var instances = services.Select(Instance.For).ToArray();
            
            return Append(instances, decoration);
        }

        public AppendState Append(Instance[] instances, IDecoratorPolicy[] decoration)
        {
            instances = applyDecorators(decoration, instances).ToArray();
            
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
            return Instances.ContainsKey(name) ? Instances[name] : _instances.Values.ToArray().FirstOrDefault(x => x.Name == name);
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

        public IReadOnlyDictionary<string, Instance> Instances => _instances;
        
        // Used internally to explain why the service type cannot be resolved
        public string CannotBeResolvedMessage { get; set; }


        /// <summary>
        /// If the ServiceType is an open generic type, this method will create a
        /// closed type copy of this PluginFamily
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="decoration"></param>
        /// <param name="templateTypes"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public ServiceFamily CreateTemplatedClone(Type serviceType, IDecoratorPolicy[] decoration, Type[] templateTypes)
        {
            if (!ServiceType.IsGenericType) throw new InvalidOperationException($"{ServiceType.FullNameInCode()} is not an open generic type");

            var instances = _instances.Values.ToArray().Select(x => {
                var clone = x.CloseType(serviceType, templateTypes);
                if (clone == null) return null;

                clone.Name = x.Name;
                return clone;
            }).Where(x => x != null).ToArray();

            return new ServiceFamily(serviceType, decoration, instances);
        }


    }

    public enum AppendState
    {
        NewDefault,
        SameDefault
    }
}
