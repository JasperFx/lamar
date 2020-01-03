using System.Linq;
using Lamar.Scanning.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class scanning_samples
    {
        // SAMPLE: WithDefaultConventions
        public interface ISpaceship { }

        public class Spaceship : ISpaceship { }

        public interface IRocket { }

        public class Rocket : IRocket { }

        [Fact]
        public void default_scanning_in_action()
        {
            var container = new Container(_ =>
            {
                _.Scan(x =>
                {
                    x.Assembly("Lamar.Testing");
                    x.WithDefaultConventions();
                });
            });

            container.GetInstance<ISpaceship>().ShouldBeOfType<Spaceship>();
            container.GetInstance<IRocket>().ShouldBeOfType<Rocket>();
        }

        // ENDSAMPLE
        
        [Fact]
        public void default_scanning_in_action_with_overrides()
        {
            // SAMPLE: WithDefaultConventionsOptions
            var container = new Container(_ =>
            {
                _.Scan(x =>
                {
                    x.Assembly("Lamar.Testing");
                    
                    // This is the default, add all discovered registrations
                    // regardless of existing registrations
                    x.WithDefaultConventions(OverwriteBehavior.Always);
                    
                    // Do not add any registrations if the *ServiceType*
                    // is already registered. This will prevent the scanning
                    // from overwriting existing default registrations
                    x.WithDefaultConventions(OverwriteBehavior.Never);
                    
                    // Only add new ImplementationType registrations for 
                    // the ServiceType. This will prevent duplicate concrete
                    // types for the same ServiceType being registered by the
                    // type scanning
                    x.WithDefaultConventions(OverwriteBehavior.NewType);
                });
            });
            // ENDSAMPLE

        }
        
        [Fact]
        public void default_scanning_in_action_with_override_lifetime()
        {
            // SAMPLE: WithDefaultConventionsLifetime
            var container = new Container(_ =>
            {
                _.Scan(x =>
                {
                    x.Assembly("Lamar.Testing");
                    
                    // Use Scoped as the lifetime
                    x.WithDefaultConventions(ServiceLifetime.Scoped);
                    
                    // Mix and match with override behavior
                    x.WithDefaultConventions(OverwriteBehavior.Never, ServiceLifetime.Singleton);
                });
            });
            // ENDSAMPLE

        }

        // SAMPLE: register-all-types-implementing
        public interface IFantasySeries { }

        public class WheelOfTime : IFantasySeries { }

        public class GameOfThrones : IFantasySeries { }

        public class BlackCompany : IFantasySeries { }

        [Fact]
        public void register_all_types_of_an_interface()
        {
            var container = new Container(_ =>
            {
                _.Scan(x =>
                {
                    x.TheCallingAssembly();

                    x.AddAllTypesOf<IFantasySeries>()
                        .NameBy(type => type.Name.ToLower());

                    // or

                    x.AddAllTypesOf(typeof(IFantasySeries))
                        .NameBy(type => type.Name.ToLower());
                });
            });

            container.Model.For<IFantasySeries>()
                .Instances.Select(x => x.ImplementationType)
                .OrderBy(x => x.Name)
                .ShouldHaveTheSameElementsAs(typeof(BlackCompany), typeof(GameOfThrones), typeof(WheelOfTime));

            container.GetInstance<IFantasySeries>("blackcompany").ShouldBeOfType<BlackCompany>();
        }

        // ENDSAMPLE

        // SAMPLE: SingleImplementationsOfInterface
        public interface ISong { }

        public class TheOnlySong : ISong { }

        [Fact]
        public void only_implementation()
        {
            var container = new Container(_ =>
            {
                _.Scan(x =>
                {
                    x.TheCallingAssembly();
                    x.SingleImplementationsOfInterface();
                });
            });

            container.GetInstance<ISong>()
                .ShouldBeOfType<TheOnlySong>();
        }

        // ENDSAMPLE
    }
}