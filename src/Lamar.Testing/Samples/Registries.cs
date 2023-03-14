namespace Lamar.Testing.Samples;

#region sample_foobar-registry

public class FooBarRegistry : ServiceRegistry
{
    public FooBarRegistry()
    {
        For<IFoo>().Use<Foo>();
        For<IBar>().Use<Bar>();
    }
}

#endregion