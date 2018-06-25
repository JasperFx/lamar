using System;
using System.Linq;
using Baseline;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class custom_policies
    {
        // SAMPLE: database-users
        public class DatabaseUser
        {
            public string ConnectionString { get; set; }

            public DatabaseUser(string connectionString)
            {
                ConnectionString = connectionString;
            }
        }

        public class ConnectedThing
        {
            public string ConnectionString { get; set; }

            public ConnectedThing(string connectionString)
            {
                ConnectionString = connectionString;
            }
        }

        // ENDSAMPLE

        // SAMPLE: connectionstringpolicy
        public class ConnectionStringPolicy : ConfiguredInstancePolicy
        {
            protected override void apply(IConfiguredInstance instance)
            {
                var parameter = instance.ImplementationType
                    .GetConstructors()
                    .SelectMany(x => x.GetParameters())
                    .FirstOrDefault(x => x.Name == "connectionString");
                
                if (parameter != null)
                {
                    var connectionString = findConnectionStringFromConfiguration();
                    instance.Ctor<string>(parameter.Name).Is(connectionString);
                }
            }

            // find the connection string from whatever configuration
            // strategy your application uses
            private string findConnectionStringFromConfiguration()
            {
                return "the connection string";
            }
        }

        // ENDSAMPLE

        // SAMPLE: use_the_connection_string_policy
        [Fact]
        public void use_the_connection_string_policy()
        {
            var container = new Container(_ =>
            {
                _.Policies.Add<ConnectionStringPolicy>();
            });

            container.GetInstance<DatabaseUser>()
                .ConnectionString.ShouldBe("the connection string");

            container.GetInstance<ConnectedThing>()
                .ConnectionString.ShouldBe("the connection string");
        }

        // ENDSAMPLE

        // SAMPLE: IDatabase
        public interface IDatabase { }

        public class Database : IDatabase
        {
            public string ConnectionString { get; set; }

            public Database(string connectionString)
            {
                ConnectionString = connectionString;
            }

            public override string ToString()
            {
                return string.Format("ConnectionString: {0}", ConnectionString);
            }
        }

        // ENDSAMPLE

        // SAMPLE: database-users-2
        public class BigService
        {
            public BigService(IDatabase green)
            {
                DB = green;
            }

            public IDatabase DB { get; set; }
        }

        public class ImportantService
        {
            public ImportantService(IDatabase red)
            {
                DB = red;
            }

            public IDatabase DB { get; set; }
        }

        public class DoubleDatabaseUser
        {
            public DoubleDatabaseUser(IDatabase red, IDatabase green)
            {
                Red = red;
                Green = green;
            }

            // Watch out for potential conflicts between setters
            // and ctor params. The easiest thing is to just make
            // setters private
            public IDatabase Green { get; private set; }

            public IDatabase Red { get; private set; }
        }

        // ENDSAMPLE

        // SAMPLE: InjectDatabaseByName
        public class InjectDatabaseByName : ConfiguredInstancePolicy
        {
            protected override void apply(IConfiguredInstance instance)
            {
                instance.ImplementationType.GetConstructors()
                    .SelectMany(x => x.GetParameters())
                    .Where(x => x.ParameterType == typeof(IDatabase))
                    .Each(param =>
                    {
                        // Using ReferencedInstance here tells Lamar
                        // to "use the IDatabase by this name"
                        instance.Ctor<IDatabase>(param.Name).IsNamedInstance(param.Name);
                    });
            }
        }

        // ENDSAMPLE

        [Fact]
        public void choose_database()
        {
            // SAMPLE: choose_database_container_setup
            var container = new Container(_ =>
            {
                _.For<IDatabase>().Add<Database>().Named("red")
                    .Ctor<string>("connectionString").Is("*red*");

                _.For<IDatabase>().Add<Database>().Named("green")
                    .Ctor<string>("connectionString").Is("*green*");

                _.Policies.Add<InjectDatabaseByName>();
            });
            // ENDSAMPLE

            // SAMPLE: inject-database-by-name-in-usage
            // ImportantService should get the "red" database
            container.GetInstance<ImportantService>()
                .DB.As<Database>().ConnectionString.ShouldBe("*red*");
            
            // BigService should get the "green" database
            container.GetInstance<BigService>()
                .DB.As<Database>().ConnectionString.ShouldBe("*green*");
            
            // DoubleDatabaseUser gets both
            var user = container.GetInstance<DoubleDatabaseUser>();

            user.Green.As<Database>().ConnectionString.ShouldBe("*green*");
            user.Red.As<Database>().ConnectionString.ShouldBe("*red*");
            // ENDSAMPLE
        }

        public interface IWidgets { }

        public class WidgetCache : IWidgets { }

        // SAMPLE: CacheIsSingleton
        public class CacheIsSingleton : IInstancePolicy
        {
            public void Apply(Instance instance)
            {
                if (instance.ImplementationType.Name.EndsWith("Cache"))
                {
                    instance.Lifetime = ServiceLifetime.Singleton;
                }
            }
        }

        // ENDSAMPLE

        // SAMPLE: set_cache_to_singleton
        [Fact]
        public void set_cache_to_singleton()
        {
            var container = new Container(_ =>
            {
                _.Policies.Add<CacheIsSingleton>();

                _.For<IWidgets>().Use<WidgetCache>();
            });

            // The policy is applied *only* at the time
            // that StructureMap creates a "build plan"
            container.GetInstance<IWidgets>()
                .ShouldBeSameAs(container.GetInstance<IWidgets>());

            // Now that the policy has executed, we
            // can verify that WidgetCache is a SingletonThing
            container.Model.For<IWidgets>().Default
                    .Lifetime.ShouldBe(ServiceLifetime.Singleton);
        }

        // ENDSAMPLE

        public class MyCustomPolicy : IInstancePolicy
        {
            public void Apply(Instance instance)
            {
            }
        }

        [Fact]
        public void show_registration()
        {
            // SAMPLE: policies.add
            var container = new Container(_ =>
            {
                _.Policies.Add<MyCustomPolicy>();
                // or
                _.Policies.Add(new MyCustomPolicy());
            });
            // ENDSAMPLE
        }


        public class WidgetFilledPolicy : IRegistrationPolicy
        {
            public void Apply(ServiceRegistry services)
            {
                services.ForSingletonOf<IWidget>().Use<AWidget>();
            }
        }

        [Fact]
        public void use_registration_policy()
        {
            var container = new Container(x =>
            {
                x.Policies.Add<WidgetFilledPolicy>();
            });

            container.GetInstance<IWidget>()
                .ShouldBeOfType<AWidget>();
        }
    }
}