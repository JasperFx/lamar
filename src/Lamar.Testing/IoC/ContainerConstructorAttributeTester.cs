using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC
{
    public class ContainerConstructorAttributeTester
    {
        [Fact]
        public void GetConstructor()
        {
            var constructor = DefaultConstructorAttribute.GetConstructor(
                typeof(ComplexRule));

            constructor.ShouldNotBeNull();
        }
    }
}