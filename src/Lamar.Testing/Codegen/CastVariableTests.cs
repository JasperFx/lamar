using Lamar.Codegen;
using Lamar.Codegen.Variables;
using Shouldly;
using Xunit;

namespace Lamar.Testing.Codegen
{
    public class CastVariableTests
    {
        [Fact]
        public void does_the_cast()
        {
            var inner = Variable.For<Basketball>();
            var cast = new CastVariable(inner, typeof(Ball));
            
            cast.Usage.ShouldBe($"(({typeof(Ball).FullNameInCode()}){inner.Usage})");
        }
    }
}