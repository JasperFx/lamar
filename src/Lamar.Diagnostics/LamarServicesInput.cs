using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JasperFx.StringExtensions;
using Lamar.IoC.Diagnostics;
using Oakton;

[assembly:OaktonCommandAssembly]

namespace Lamar.Diagnostics
{
    
    public class LamarServicesInput : NetCoreInput
    {
        [Description("Optional file to write the results")]
        public string FileFlag { get; set; }

        [Description("Optionally filter the results to only types in this namespace")]
        public string NamespaceFlag { get; set; }
        
        [Description("Optionally filter the results to only types in this assembly")]
        public string AssemblyFlag { get; set; }
        
        [Description("Optionally filter the results to only this named type. Can be either a type name or a full name")]
        public string TypeFlag { get; set; }

        [Description("Show the full build plans")]
        public bool BuildPlansFlag { get; set; }
        

        public ModelQuery ToModelQuery(IContainer container)
        {
            var query = new ModelQuery
            {
                Namespace = NamespaceFlag,
                TypeName = TypeFlag
                    
            };
                
            if (AssemblyFlag.IsNotEmpty())
            {
                query.Assembly = tryFindAssembly(AssemblyFlag, container);
            }

            return query;
        }
        
        private Assembly tryFindAssembly(string assemblyName, IContainer container)
        {
            var assemblies = container.Model.AllInstances.Select(x => x.ImplementationType.Assembly)
                .Concat(container.Model.AllInstances.Select(x => x.ServiceType.Assembly))
                .Distinct();

            return assemblies.FirstOrDefault(x => x.GetName().Name.EqualsIgnoreCase(assemblyName));
        }

        public IEnumerable<IServiceFamilyConfiguration> Query(IContainer container)
        {
            var query = ToModelQuery(container);
            var configurations = query.Query(container.Model);
            if (!VerboseFlag)
            {
                configurations = configurations.Where(x => !x.ServiceType.IgnoreIfNotVerbose());
            }

            return configurations;
        }
    }
}