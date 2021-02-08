using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LamarCodeGeneration.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Lamar.Scanning.Conventions
{
    internal class ScanningExploder
    {
        internal static (ServiceRegistry, AssemblyScanner[]) ExplodeSynchronously(IServiceCollection services)
        {
            var scanners = new AssemblyScanner[0];
            var registry = new ServiceRegistry(services);

            var registriesEncountered = new List<Type>();

            while (registry.HasScanners())
            {
                var (registry2, operations) = ParseToOperations(registry, registriesEncountered);

                var additional = operations.OfType<AssemblyScanner>().ToArray();

                registry = registry2;
                scanners = scanners.Concat(additional).ToArray();

                registry.RemoveAll(x => x.ServiceType == typeof(AssemblyScanner));

                Task.WhenAll(additional.Select(x => x.TypeFinder)).Wait(TimeSpan.FromSeconds(2));

                foreach (var operation in operations)
                {
                    if (operation is AssemblyScanner scanner) scanner.ApplyRegistrations(registry);

                    if (operation is ServiceDescriptor[] descriptors) registry.AddRange(descriptors);
                }
            }

            return (registry, scanners);
        }

        internal static async Task<(ServiceRegistry, AssemblyScanner[])> Explode(IServiceCollection services)
        {
            var scanners = new AssemblyScanner[0];
            var registry = new ServiceRegistry(services);
            
            var registriesEncountered = new List<Type>();

            while (registry.HasScanners())
            {
                var (registry2, operations) = ParseToOperations(registry, registriesEncountered);

                var additional = operations.OfType<AssemblyScanner>().ToArray();

                registry = registry2;
                scanners = scanners.Concat(additional).ToArray();

                registry.RemoveAll(x => x.ServiceType == typeof(AssemblyScanner));

                foreach (var operation in operations)
                {
                    if (operation is AssemblyScanner scanner)
                    {
                        await scanner.TypeFinder;
                        scanner.ApplyRegistrations(registry);
                    }

                    if (operation is ServiceDescriptor[] descriptors) registry.AddRange(descriptors);
                }
            }

            return (registry, scanners);
        }

        internal static (ServiceRegistry, List<object>) ParseToOperations(IServiceCollection services,
            List<Type> registriesEncountered)
        {
            var scanners = services
                .Where(x => x.ServiceType == typeof(AssemblyScanner))
                .ToArray();

            var indexes = scanners
                .Select(services.IndexOf)
                .ToArray();

            var operations = new List<object>();

            var initial = indexes[0] > 0
                ? new ServiceRegistry(services.Take(indexes[0]))
                : new ServiceRegistry();

            initial.RegistryTypes = registriesEncountered;

            operations.Add(scanners[0].ImplementationInstance);

            for (var i = 1; i < indexes.Length; i++)
            {
                var index = indexes[i];
                var previous = indexes[i - 1];

                if (previous != index - 1)
                {
                    // they are not sequential, just add a Scan operation
                    var slice = services.Skip(previous + 1).Take(index - previous - 1).ToArray();
                    operations.Add(slice);
                }


                operations.Add(scanners[i].ImplementationInstance);
            }

            // Are there more?
            if (indexes.Last() != services.Count - 1) operations.Add(services.Skip(indexes.Last() + 1).ToArray());

            return (initial, operations);
        }
    }
}