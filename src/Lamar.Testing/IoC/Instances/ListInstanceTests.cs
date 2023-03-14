using System.Collections.Generic;
using Lamar.IoC.Enumerables;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Lamar.Testing.IoC.Instances;

public class ListInstanceTests
{
    [Fact]
    public void shoule_get_instance_with_inline_dependencies()
    {
        var instance = new ListInstance<string>(typeof(IList<string>));
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
        IList<string> Data { get; }
    }

    private class TestInstance : ITestInstance
    {
        public TestInstance(IList<string> data)
        {
            Data = data;
        }

        public IList<string> Data { get; }
    }
}