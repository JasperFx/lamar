using Shouldly;
using StructureMap.Graph;

namespace StructureMap.Docs.samples
{
    public interface IBlah
    {
    }

    public interface IService
    {
        void DoSomething();
    }


#region sample_foobar-model-structuremap
    public interface IBar
    {
    }

    public class Bar : IBar
    {
    }

    public interface IFoo
    {
    }

    public class Foo : IFoo
    {
        public IBar Bar { get; private set; }

        public Foo(IBar bar)
        {
            Bar = bar;
        }
    }

#endregion


    public static class GoFooBarModel
    {
        public static void Quickstart1()
        {
#region sample_foobar-quickstart1
// Configure and build a brand new
// StructureMap Container object
            var container = new Container(_ =>
            {
                _.For<IFoo>().Use<Foo>();
                _.For<IBar>().Use<Bar>();
            });

// Now, resolve a new object instance of IFoo
            container.GetInstance<IFoo>()
                // should be type Foo
                .ShouldBeOfType<Foo>()

                // and the IBar dependency too
                .Bar.ShouldBeOfType<Bar>();


#endregion
        }

        public static void Quickstart2()
        {
#region sample_foobar-quickstart2
            var container = new Container(_ =>
            {
                _.Scan(x =>
                {
                    x.TheCallingAssembly();
                    x.WithDefaultConventions();
                });
            });

            container.GetInstance<IFoo>()
                .ShouldBeOfType<Foo>()
                .Bar.ShouldBeOfType<Bar>();

#endregion
        }
    }

    public class SomeOtherFoo : IFoo
    {
    }


#region sample_concrete-weather-model
    public class Weather
    {
        public Location Location { get; set; }
        public Atmosphere Atmosphere { get; set; }
        public Wind Wind { get; set; }
        public Condition Condition { get; set; }

        public Weather(Location location, Atmosphere atmosphere, Wind wind, Condition condition)
        {
            Location = location;
            Atmosphere = atmosphere;
            Wind = wind;
            Condition = condition;
        }
    }

    public class Location
    {
        //some properties
    }

    public class Atmosphere
    {
        //some properties
    }

    public class Wind
    {
        //some properties        
    }

    public class Condition
    {
        //some properties        
    }

#endregion
}
