using System;
using Lamar.IoC.Instances;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class register_a_custom_instance
    {
        [Fact]
        public void can_work()
        {
            var container = new Container(x =>
                {
                    x.For(typeof(IWidget)).Use(new AWidgetInstance(typeof(IWidget), ServiceLifetime.Scoped));
                });

            container.GetInstance<IWidget>()
                .ShouldBeOfType<AWidget>();
        }

        public class AWidgetInstance : ConstructorInstance<AWidget, IWidget>
        {
            public AWidgetInstance(Type serviceType, ServiceLifetime lifetime) : base(serviceType, lifetime)
            {
            }
        }
    }
}