using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
#if NET6_0_OR_GREATER
    public class IServiceProviderIsService_implementation
    {
        [Fact]
        public void concrete_types_must_be_explicitly_registered()
        {
            var container = Container.For(x =>
            {
                x.AddSingleton<AWidget>();

            });
            
            container.IsService(typeof(AWidget)).ShouldBeTrue();
            container.IsService(typeof(BWidget)).ShouldBeFalse();
            
        }

        [Fact]
        public void explicit_checks_of_non_concrete_types()
        {
            var container = Container.For(x =>
            {
                x.For<IClock>().Use<Clock>();
            });
            
            container.IsService(typeof(IClock)).ShouldBeTrue();
            container.IsService(typeof(IWidget)).ShouldBeFalse();
        }
    }
#endif
}