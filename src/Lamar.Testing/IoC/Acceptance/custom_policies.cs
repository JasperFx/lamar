﻿using System.Linq;
using JasperFx.Reflection;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance;

public class custom_policies
{
    #region sample_use_the_connection_string_policy

    [Fact]
    public void use_the_connection_string_policy()
    {
        var container = new Container(_ => { _.Policies.Add<ConnectionStringPolicy>(); });

        container.GetInstance<DatabaseUser>()
            .ConnectionString.ShouldBe("the connection string");

        container.GetInstance<ConnectedThing>()
            .ConnectionString.ShouldBe("the connection string");
    }

    #endregion

    [Fact]
    public void choose_database()
    {
        #region sample_choose_database_container_setup

        var container = new Container(_ =>
        {
            _.For<IDatabase>().Add<Database>().Named("red")
                .Ctor<string>("connectionString").Is("*red*");

            _.For<IDatabase>().Add<Database>().Named("green")
                .Ctor<string>("connectionString").Is("*green*");

            _.Policies.Add<InjectDatabaseByName>();
        });

        #endregion

        #region sample_inject-database-by-name-in-usage

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

        #endregion
    }

    #region sample_set_cache_to_singleton

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

    #endregion

    [Fact]
    public void show_registration()
    {
        #region sample_policies.add

        var container = new Container(_ =>
        {
            _.Policies.Add<MyCustomPolicy>();
            // or
            _.Policies.Add(new MyCustomPolicy());
        });

        #endregion
    }

    [Fact]
    public void use_registration_policy()
    {
        var container = new Container(x => { x.Policies.Add<WidgetFilledPolicy>(); });

        container.GetInstance<IWidget>()
            .ShouldBeOfType<AWidget>();
    }

    #region sample_connectionstringpolicy

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

    #endregion

    #region sample_InjectDatabaseByName

    public class InjectDatabaseByName : ConfiguredInstancePolicy
    {
        protected override void apply(IConfiguredInstance instance)
        {
            var parameterInfos = instance.ImplementationType.GetConstructors()
                .SelectMany(x => x.GetParameters())
                .Where(x => x.ParameterType == typeof(IDatabase));

            foreach (var param in parameterInfos)
                // Using ReferencedInstance here tells Lamar
                // to "use the IDatabase by this name"
                instance.Ctor<IDatabase>(param.Name).IsNamedInstance(param.Name);
        }
    }

    #endregion

    public interface IWidgets
    {
    }

    public class WidgetCache : IWidgets
    {
    }

    #region sample_CacheIsSingleton

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

    #endregion

    public class MyCustomPolicy : IInstancePolicy
    {
        public void Apply(Instance instance)
        {
        }
    }


    public class WidgetFilledPolicy : IRegistrationPolicy
    {
        public void Apply(ServiceRegistry services)
        {
            services.ForSingletonOf<IWidget>().Use<AWidget>();
        }
    }

    #region sample_database-users

    public class DatabaseUser
    {
        public DatabaseUser(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; set; }
    }

    public class ConnectedThing
    {
        public ConnectedThing(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; set; }
    }

    #endregion

    #region sample_IDatabase

    public interface IDatabase
    {
    }

    public class Database : IDatabase
    {
        public Database(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; set; }

        public override string ToString()
        {
            return string.Format("ConnectionString: {0}", ConnectionString);
        }
    }

    #endregion

    #region sample_database-users-2

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
        public IDatabase Green { get; }

        public IDatabase Red { get; }
    }

    #endregion
}