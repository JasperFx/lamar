using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.Bugs
{
    public class Test
    {
        [Fact]
        public void Should_resolve_ctor_injection()
        {
            var container = new Container(x =>
            {
                var serviceDescriptor = ServiceDescriptor.Transient(typeof(IWidget), provider => provider.GetRequiredService(typeof(AWidget)));
                x.Add(serviceDescriptor);
            });

            var result = container.GetInstance<MyClass>();

            result.ShouldBeOfType<MyClass>();
        }

        private class MyClass
        {
            public MyClass(IWidget widget)
            {
            }
        }
    }
}