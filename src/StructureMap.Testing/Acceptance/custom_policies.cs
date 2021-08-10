﻿using Shouldly;
using StructureMap.Pipeline;
using System;
using System.Linq;
using Xunit;

namespace StructureMap.Testing.Acceptance
{
    public class custom_policies
    {
        #region sample_database-users
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

        #endregion

        #region sample_connectionstringpolicy
        public class ConnectionStringPolicy : ConfiguredInstancePolicy
        {
            protected override void apply(Type pluginType, IConfiguredInstance instance)
            {
                var parameter = instance.Constructor.GetParameters().FirstOrDefault(x => x.Name == "connectionString");
                if (parameter != null)
                {
                    var connectionString = findConnectionStringFromConfiguration();
                    instance.Dependencies.AddForConstructorParameter(parameter, connectionString);
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

        #region sample_use_the_connection_string_policy
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

        #endregion

        #region sample_IDatabase
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
            public IDatabase Green { get; private set; }

            public IDatabase Red { get; private set; }
        }

        #endregion

        #region sample_InjectDatabaseByName
        public class InjectDatabaseByName : ConfiguredInstancePolicy
        {
            protected override void apply(Type pluginType, IConfiguredInstance instance)
            {
                instance.Constructor.GetParameters()
                    .Where(x => x.ParameterType == typeof(IDatabase))
                    .Each(param =>
                    {
                        // Using ReferencedInstance here tells StructureMap
                        // to "use the IDatabase by this name"
                        var db = new ReferencedInstance(param.Name);
                        instance.Dependencies.AddForConstructorParameter(param, db);
                    });
            }
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

        public interface IWidgets { }

        public class WidgetCache : IWidgets { }

        #region sample_CacheIsSingleton
        public class CacheIsSingleton : IInstancePolicy
        {
            public void Apply(Type pluginType, Instance instance)
            {
                if (instance.ReturnedType.Name.EndsWith("Cache"))
                {
                    instance.SetLifecycleTo<SingletonLifecycle>();
                }
            }
        }

        #endregion

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
                .ShouldBeTheSameAs(container.GetInstance<IWidgets>());

            // Now that the policy has executed, we
            // can verify that WidgetCache is a SingletonThing
            container.Model.For<IWidgets>().Default
                .Lifecycle.ShouldBeOfType<SingletonLifecycle>();
        }

        #endregion

        public class MyCustomPolicy : IInstancePolicy
        {
            public void Apply(Type pluginType, Instance instance)
            {
            }
        }

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
    }
}