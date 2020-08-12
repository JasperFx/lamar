using Lamar.IoC.Enumerables;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Lamar.Testing.IoC.Instances
{
    public class ListInstanceTests
    {
        [Fact]
        public void shoule_get_instance_with_inline_dependencies()
        {
            ListInstance<string> instance = new ListInstance<string>(typeof(IList<string>));
            instance.AddInline(new ObjectInstance(typeof(string), "a"));
            instance.AddInline(new ObjectInstance(typeof(string), "b"));

            ConstructorInstance @object = new ConstructorInstance(typeof(ITestInstance), typeof(TestInstance), ServiceLifetime.Transient);
            @object.AddInline(instance);

            IContainer container = new Container(p =>
            {
                p.For<ITestInstance>().Use(@object);
            });


            ITestInstance value = container.GetInstance<ITestInstance>();
            value.Data.ShouldHaveTheSameElementsAs(new string[] { "a", "b" });
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
}
