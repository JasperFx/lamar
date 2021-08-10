using StructureMap.Configuration.DSL;

namespace StructureMap.Docs.samples
{
#region sample_foobar-registry-structuremap
    public class FooBarRegistry : Registry
    {
        public FooBarRegistry()
        {
            For<IFoo>().Use<Foo>();
            For<IBar>().Use<Bar>();
        }
    }

#endregion

#region sample_foo-registry
    public class FooRegistry : Registry
    {
        public FooRegistry()
        {
            For<IFoo>().Use<Foo>();
        }
    }

#endregion
}