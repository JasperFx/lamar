using System.Linq;
using JasperFx.Core.TypeScanning;
using Lamar.Scanning.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance;

public class custom_registration_convention
{
    #region sample_custom-registration-convention

    public interface IFoo
    {
    }

    public interface IBar
    {
    }

    public interface IBaz
    {
    }

    public class BusyGuy : IFoo, IBar, IBaz
    {
    }

    // Custom IRegistrationConvention
    public class AllInterfacesConvention : IRegistrationConvention
    {
        public void ScanTypes(TypeSet types, ServiceRegistry services)
        {
            // Only work on concrete types
            foreach (var type in types.FindTypes(TypeClassification.Concretes | TypeClassification.Closed)
                         .Where(x => x.Name == "BusyGuy"))
            {
                // Register against all the interfaces implemented
                // by this concrete class

                foreach (var @interface in type.GetInterfaces()) services.AddTransient(@interface, type);
            }

            ;
        }
    }

    [Fact]
    public void use_custom_registration_convention()
    {
        var container = new Container(_ =>
        {
            _.Scan(x =>
            {
                // You're probably going to want to tightly filter
                // the Type's that are applicable to avoid unwanted
                // registrations
                x.TheCallingAssembly();
                x.IncludeNamespaceContainingType<BusyGuy>();

                // Register the custom policy
                x.Convention<AllInterfacesConvention>();
            });
        });

        container.GetInstance<IFoo>().ShouldBeOfType<BusyGuy>();
        container.GetInstance<IBar>().ShouldBeOfType<BusyGuy>();
        container.GetInstance<IBaz>().ShouldBeOfType<BusyGuy>();
    }

    #endregion
}