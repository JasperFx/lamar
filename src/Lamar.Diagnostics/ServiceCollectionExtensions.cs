using JasperFx.Core.TypeScanning;
using Microsoft.Extensions.DependencyInjection;
using Oakton.Environment;

namespace Lamar.Diagnostics
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an environment check to validate all the Lamar IoC configuration, including Lamar's own
        /// environment checks
        /// <param name="mode">Specify the level of the environment checks. Default is Full</param>
        /// </summary>
        public static void CheckLamarConfiguration(this IServiceCollection services, AssertMode mode = AssertMode.Full)
        {
            services.CheckEnvironment("Lamar IoC Service Registrations",s => ((IContainer)s).AssertConfigurationIsValid(mode));
            services.CheckEnvironment("Lamar IoC Type Scanning", s => TypeRepository.AssertNoTypeScanningFailures());
        }
    }

}