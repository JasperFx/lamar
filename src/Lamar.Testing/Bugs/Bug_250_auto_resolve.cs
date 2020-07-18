using Shouldly;
using Xunit;

namespace Lamar.Testing.Bugs
{
    public class Bug_250_auto_resolve
    {
        public class TestClass {}
        
        [Fact]
        public void can_auto_resolve_concrete_type()
        {
            var container = new Container(_ => { });

            container.GetInstance<TestClass>().ShouldNotBeNull();
        }
    }
}