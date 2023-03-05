using System;
using System.Collections.Generic;
using System.Linq;
using JasperFx.Core;
using JasperFx.CodeGeneration.Model;
using Microsoft.Extensions.DependencyInjection;
using JasperFx.CodeGeneration.Util;

namespace Lamar.IoC.Frames
{
    public class ServiceVariableSource : IServiceVariableSource
    {
        private readonly ServiceGraph _services;
        private readonly IList<ServiceStandinVariable> _standins = new List<ServiceStandinVariable>();

        private readonly IList<InjectedServiceField> _fields = new List<InjectedServiceField>();
        private bool _usesNestedContainerDirectly;
        private Variable _nested = new NestedContainerCreation().Nested;

        public ServiceVariableSource(ServiceGraph services)
        {
            _services = services;
        }

        public bool Matches(Type type)
        {
            return _services.CouldResolve(type);
        }

        public Variable Create(Type type)
        {
            if (type == typeof(IContainer))
            {
                _usesNestedContainerDirectly = true;
                return _nested;
            }

            if (type == typeof(IServiceProvider))
            {
                _usesNestedContainerDirectly = true;
                return new CastVariable(_nested, typeof(IServiceProvider));
            }
            
            var instance = _services.FindDefault(type);
            if (instance.Lifetime == ServiceLifetime.Singleton)
            {
                var field = _fields.FirstOrDefault(x => x.Instance == instance);
                if (field == null)
                {
                    field = new InjectedServiceField(instance);
                    _fields.Add(field);
                }

                return field;
            }
            
            var standin =  new ServiceStandinVariable(instance);
            _standins.Add(standin);
            
            return standin;
        }

        public void ReplaceVariables(IMethodVariables method)
        {
            if (_usesNestedContainerDirectly || _standins.Any(x => x.Instance.RequiresServiceProvider(method)))
            {
                useServiceProvider(method);
            }
            else
            {
                useInlineConstruction(method);
            }
        }

        public void StartNewType()
        {
            StartNewMethod();
            _fields.Clear();
        }

        public void StartNewMethod()
        {
            _nested = new NestedContainerCreation().Nested;
            _standins.Clear();
        }

        private void useInlineConstruction(IMethodVariables method)
        {
            // THIS NEEDS TO BE SCOPED PER METHOD!!!
            var variables = new ResolverVariables(method, _fields);
            foreach (var standin in _standins)
            {
                var variable = variables.Resolve(standin.Instance, BuildMode.Inline);
                standin.UseInner(variable);
            }

            variables.OfType<InjectedServiceField>().Each(field =>
            {
                var family = _services.FindAll(field.VariableType);
                field.IsOnlyOne = family.Length == 1;
            });
            
            variables.MakeNamesUnique();
        }

        private void useServiceProvider(IMethodVariables method)
        {
            foreach (var standin in _standins)
            {
                var variable = new GetInstanceFromNestedContainerFrame(_nested, standin.VariableType).Variable;
                standin.UseInner(variable);
            }
        }
    }
}
