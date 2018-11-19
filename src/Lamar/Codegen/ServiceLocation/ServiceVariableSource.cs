using System;
using System.Collections.Generic;
using System.Linq;
using Lamar.Codegen.Variables;
using Lamar.IoC;
using Lamar.IoC.Frames;
using Lamar.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.Codegen.ServiceLocation
{
    public interface IServiceVariableSource : IVariableSource
    {
        void ReplaceVariables();
    }

    public class ServiceVariableSource : IServiceVariableSource
    {
        private readonly ServiceGraph _services;
        private readonly IList<ServiceStandinVariable> _standins = new List<ServiceStandinVariable>();

        private readonly IList<InjectedServiceField> _fields = new List<InjectedServiceField>();

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
            var instance = _services.FindDefault(type);
            if (instance.Lifetime == ServiceLifetime.Singleton)
            {
                var field = new InjectedServiceField(instance);
                _fields.Add(field);

                return field;
            }


            var standin =  new ServiceStandinVariable(instance);
            _standins.Add(standin);


            return standin;
        }

        // TODO -- later, do we use other variables?
        public void ReplaceVariables()
        {
            if (_standins.Any(x => x.Instance.RequiresServiceProvider))
            {
                useServiceProvider();
            }
            else
            {
                useInlineConstruction();
            }
        }

        private void useInlineConstruction()
        {
            var variables = new ResolverVariables(_fields);
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

        private void useServiceProvider()
        {
            var factory = new InjectedField(typeof(IServiceScopeFactory));
            var createScope = new ServiceScopeFactoryCreation(factory);
            var provider = createScope.Provider;

            foreach (var standin in _standins)
            {
                var variable = new GetServiceFrame(provider, standin.VariableType).Variable;
                standin.UseInner(variable);
            }
        }
    }
}
