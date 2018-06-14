using Baseline;
using Lamar.Testing.IoC.Acceptance;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.Bugs
{
    public class Bug_47_lambda_as_inline_dependency
    {
        public class WidgetBuilder
        {
            public IWidget Build()
            {
                return new AWidget();
            }
        }

        [Fact]
        public void should_be_okay()
        {
            var container = new Container(_ =>
            {
                _.For<WidgetUser>().Add<WidgetUser>()
                    .Ctor<IWidget>().Is(c => c.GetInstance<WidgetBuilder>().Build());
            });

            container.GetInstance<WidgetUser>().Widget.ShouldNotBeNull();

            var nested = container.As<IServiceScopeFactory>()
                .CreateScope();
                
            
            nested.ServiceProvider.GetService<WidgetUser>()
                .Widget.ShouldNotBeNull();
                
        }
    }
}