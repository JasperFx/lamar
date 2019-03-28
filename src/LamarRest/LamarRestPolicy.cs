using System;
using System.Linq;
using Baseline.Reflection;
using Lamar;
using Lamar.IoC.Instances;
using LamarCodeGeneration;
using LamarCompiler;
using LamarRest.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace LamarRest
{
    public class LamarRestPolicy : Lamar.IFamilyPolicy
    {
        public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
        {
            if (type.GetMethods().Any(x => x.HasAttribute<PathAttribute>()))
            {
                var rules = new GenerationRules("LamarRest");
                var generatedAssembly = new GeneratedAssembly(rules);
                var generatedType = new GeneratedServiceType(generatedAssembly, type);

                var container = (IContainer)serviceGraph.RootScope;

                var services = container.CreateServiceVariableSource();
                new AssemblyGenerator().Compile(generatedAssembly, services);
                
                return new ServiceFamily(
                    type, 
                    new IDecoratorPolicy[0], 
                    new ConstructorInstance(type, 
                    generatedType.CompiledType, 
                    ServiceLifetime.Singleton
                ));
            }

            return null;
        }
    }
}