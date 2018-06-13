using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Lamar.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable 1591

namespace Lamar.Scanning.Conventions
{
    [LamarIgnore]
    public class AssemblyScanner : IAssemblyScanner
    {
        private readonly List<Assembly> _assemblies = new List<Assembly>();
        private readonly CompositeFilter<Type> _filter = new CompositeFilter<Type>();
        private Task<TypeSet> _typeFinder;

        public AssemblyScanner()
        {
            Exclude(type => type.HasAttribute<LamarIgnoreAttribute>());
        }


        public string Description { get; set; }


        public void Assembly(Assembly assembly)
        {
            if (!_assemblies.Contains(assembly))
                _assemblies.Add(assembly);
        }

        public void Assembly(string assemblyName)
        {
            Assembly(AssemblyLoader.ByName(assemblyName));
        }

        public List<IRegistrationConvention> Conventions { get; } = new List<IRegistrationConvention>();

        public void Convention<T>() where T : IRegistrationConvention, new()
        {
            var previous = Conventions.FirstOrDefault(scanner => scanner is T);
            if (previous == null)
                With(new T());
        }

        public void AssemblyContainingType<T>()
        {
            AssemblyContainingType(typeof(T));
        }

        public void AssemblyContainingType(Type type)
        {
            _assemblies.Add(type.GetTypeInfo().Assembly);
        }

        public FindAllTypesFilter AddAllTypesOf<TPluginType>()
        {
            return AddAllTypesOf(typeof(TPluginType));
        }

        public FindAllTypesFilter AddAllTypesOf(Type pluginType)
        {
            var filter = new FindAllTypesFilter(pluginType);
            With(filter);

            return filter;
        }


        public void Exclude(Func<Type, bool> exclude)
        {
            _filter.Excludes += exclude;
        }

        public void ExcludeNamespace(string nameSpace)
        {
            Exclude(type => type.IsInNamespace(nameSpace));
        }

        public void ExcludeNamespaceContainingType<T>()
        {
            ExcludeNamespace(typeof(T).Namespace);
        }

        public void Include(Func<Type, bool> predicate)
        {
            _filter.Includes += predicate;
        }

        public void IncludeNamespace(string nameSpace)
        {
            Include(type => type.IsInNamespace(nameSpace));
        }

        public void IncludeNamespaceContainingType<T>()
        {
            IncludeNamespace(typeof(T).Namespace);
        }

        public void ExcludeType<T>()
        {
            Exclude(type => type == typeof(T));
        }

        public void With(IRegistrationConvention convention)
        {
            Conventions.Fill(convention);
        }

        public void WithDefaultConventions()
        {
            var convention = new DefaultConventionScanner();
            With(convention);
        }
        
        public void ConnectImplementationsToTypesClosing(Type openGenericType)
        {
            var convention = new GenericConnectionScanner(openGenericType);
            With(convention);
        }


        public void RegisterConcreteTypesAgainstTheFirstInterface()
        {
            var convention = new FirstInterfaceConvention();
            With(convention);
        }

        public void SingleImplementationsOfInterface()
        {
            var convention = new ImplementationMap();
            With(convention);
        }

        public void Describe(StringWriter writer)
        {
            writer.WriteLine(Description);
            writer.WriteLine("Assemblies");
            writer.WriteLine("----------");

            _assemblies.OrderBy(x => x.FullName).Each(x => writer.WriteLine("* " + x));
            writer.WriteLine();

            writer.WriteLine("Conventions");
            writer.WriteLine("--------");
            Conventions.Each(x => writer.WriteLine("* " + x));
        }

        public void Start()
        {
            if (!Conventions.Any())
            {
                throw new InvalidOperationException($"There are no {nameof(IRegistrationConvention)}'s in this scanning operation. ");
            }

            _typeFinder = TypeRepository.FindTypes(_assemblies, type => _filter.Matches(type));
        }

        
        
        public Task ApplyRegistrations(IServiceCollection services)
        {
            
            
            return _typeFinder.ContinueWith(t =>
            {
                foreach (var convention in Conventions)
                {
                    convention.ScanTypes(t.Result, services);
                }
            });
        }

        public bool Contains(string assemblyName)
        {
            return _assemblies
                .Select(assembly => new AssemblyName(assembly.FullName))
                .Any(aName => aName.Name == assemblyName);
        }


        public void TheCallingAssembly()
        {
            var callingAssembly = CallingAssembly.Find();

            if (callingAssembly != null)
            {
                Assembly(callingAssembly);
            }
            else
            {
                throw new InvalidOperationException("Could not determine the calling assembly, you may need to explicitly call IAssemblyScanner.Assembly()");
            }
        }

        public void AssembliesFromApplicationBaseDirectory()
        {
            AssembliesFromApplicationBaseDirectory(a => true);
        }

        public void AssembliesFromApplicationBaseDirectory(Func<Assembly, bool> assemblyFilter)
        {
            var assemblies = AssemblyFinder.FindAssemblies(assemblyFilter, txt =>
            {
                Console.WriteLine("Jasper could not load assembly from file " + txt);
            }, includeExeFiles: false);

            foreach (var assembly in assemblies)
            {
                Assembly(assembly);
            }
        }

        /// <summary>
        /// Choosing option will direct Jasper to *also* scan files ending in '*.exe'
        /// </summary>
        /// <param name="scanner"></param>
        /// <param name="assemblyFilter"></param>
        /// <param name="includeExeFiles"></param>
        public void AssembliesAndExecutablesFromApplicationBaseDirectory(Func<Assembly, bool> assemblyFilter = null)
        {
            var assemblies = AssemblyFinder.FindAssemblies(assemblyFilter, txt =>
            {
                Console.WriteLine("Jasper could not load assembly from file " + txt);
            }, includeExeFiles: true);

            foreach (var assembly in assemblies)
            {
                Assembly(assembly);
            }
        }

        public void AssembliesAndExecutablesFromPath(string path)
        {
            var assemblies = AssemblyFinder.FindAssemblies(path, txt =>
            {
                Console.WriteLine("Jasper could not load assembly from file " + txt);
            }, includeExeFiles: true);

            foreach (var assembly in assemblies)
            {
                Assembly(assembly);
            }
        }

        public void AssembliesFromPath(string path)
        {
            var assemblies = AssemblyFinder.FindAssemblies(path, txt =>
            {
                Console.WriteLine("Jasper could not load assembly from file " + txt);
            }, includeExeFiles: false);

            foreach (var assembly in assemblies)
            {
                Assembly(assembly);
            }
        }

        public void AssembliesAndExecutablesFromPath(string path,
            Func<Assembly, bool> assemblyFilter)
        {
            var assemblies = AssemblyFinder.FindAssemblies(path, txt =>
            {
                Console.WriteLine("Jasper could not load assembly from file " + txt);
            }, includeExeFiles: true).Where(assemblyFilter);


            foreach (var assembly in assemblies)
            {
                Assembly(assembly);
            }
        }

        public void AssembliesFromPath(string path,
            Func<Assembly, bool> assemblyFilter)
        {
            var assemblies = AssemblyFinder.FindAssemblies(path, txt =>
            {
                Console.WriteLine("Jasper could not load assembly from file " + txt);
            }, includeExeFiles: false).Where(assemblyFilter);


            foreach (var assembly in assemblies)
            {
                Assembly(assembly);
            }
        }


    }


}

#pragma warning restore 1591
