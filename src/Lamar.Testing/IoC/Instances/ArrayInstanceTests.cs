using Lamar.IoC.Enumerables;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Lamar.Testing.IoC.Instances;

public class ArrayInstanceTests
{
    [Fact]
    public void shoule_get_instance_with_inline_dependencies()
    {
        var instance = new ArrayInstance<string>(typeof(string[]));
        instance.AddInline(new ObjectInstance(typeof(string), "a"));
        instance.AddInline(new ObjectInstance(typeof(string), "b"));

        var @object = new ConstructorInstance(typeof(ITestInstance), typeof(TestInstance), ServiceLifetime.Transient);
        @object.AddInline(instance);

        IContainer container = new Container(p => { p.For<ITestInstance>().Use(@object); });


        var value = container.GetInstance<ITestInstance>();
        value.Data.ShouldHaveTheSameElementsAs("a", "b");
    }


    private interface ITestInstance
    {
        string[] Data { get; }
    }

    private class TestInstance : ITestInstance
    {
        public TestInstance(string[] data)
        {
            Data = data;
        }

        public string[] Data { get; }
    }
}