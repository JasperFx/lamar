
namespace StructureMap.Docs.samples.quickstart
{
    public interface IDbConnection
    {
        
    }

    public class SqlConnection : IDbConnection
    {
        
    }


    internal class configuring_the_container
    {
        public void configure_the_container()
        {
#region sample_quickstart-configure-the-container
// Example #1 - Create an container instance and directly pass in the configuration.
            var container1 = new Container(c =>
            {
                c.For<IFoo>().Use<Foo>();
                c.For<IBar>().Use<Bar>();
            });

// Example #2 - Create an container instance but add configuration later.
            var container2 = new Container();

            container2.Configure(c =>
            {
                c.For<IFoo>().Use<Foo>();
                c.For<IBar>().Use<Bar>();
            });
#endregion
        }

        public void configure_the_container_using_a_registry()
        {
#region sample_quickstart-configure-the-container-using-a-registry
// Example #1
            var container1 = new Container(new FooBarRegistry());

// Example #2
            var container2 = new Container(c => { c.AddRegistry<FooBarRegistry>(); });

// Example #3 -- create a container for a single Registry
            var container3 = Container.For<FooBarRegistry>();
#endregion
        }

        public void configure_the_container_using_auto_registrations_and_conventions()
        {
#region sample_quickstart-configure-the-container-using-auto-registrations-and-conventions
// Example #1
            var container1 = new Container(c =>
                c.Scan(scanner =>
                {
                    scanner.TheCallingAssembly();
                    scanner.WithDefaultConventions();
                }));

// Example #2
            var container2 = new Container();

            container2.Configure(c =>
                c.Scan(scanner =>
                {
                    scanner.TheCallingAssembly();
                    scanner.WithDefaultConventions();
                }));


#endregion
        }

        public void configure_the_container_and_provide_a_primitive()
        {
#region sample_quickstart-container-with-primitive-value
            var container = new Container(c =>
            {
                //just for demo purposes, normally you don't want to embed the connection string directly into code.
                c.For<IDbConnection>().Use<SqlConnection>().Ctor<string>().Is("YOUR_CONNECTION_STRING");
                //a better way would be providing a delegate that retrieves the value from your app config.    
            });
#endregion
        }


        public void configure_multiple_services_of_the_same_type()
        {
#region sample_quickstart-configure-multiple-services-of-the-same-type
            var container = new Container(i =>
            {
                i.For<IFoo>().Use<Foo>();
                i.For<IFoo>().Use<SomeOtherFoo>();
                i.For<IBar>().Use<Bar>();
            });
#endregion
        }
    }
}