namespace Lamar.Testing.Samples
{
// SAMPLE: foobar-registry
    public class FooBarRegistry : ServiceRegistry
    {
        public FooBarRegistry()
        {
            For<IFoo>().Use<Foo>();
            For<IBar>().Use<Bar>();
        }
    }

// ENDSAMPLE
}