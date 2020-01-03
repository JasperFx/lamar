using System;
using Lamar.Testing.IoC.Instances;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class depends_on
    {
        [Theory]
        [InlineData(typeof(Rule), true)]
        [InlineData(typeof(ARule), true)]
        [InlineData(typeof(IWidget), true)]
        [InlineData(typeof(WidgetWithRule), true)]
        [InlineData(typeof(ColorWidget), false)]
        [InlineData(typeof(BlueRule), false)]
        [InlineData(typeof(IThing), false)]
        public void depends_on_condition(Type dependencyType, bool isDependent)
        {
            var container = new Container(x =>
            {
                x.For<IWidget>().Use<WidgetWithRule>();
                x.For<Rule>().Use<ARule>();
                x.For<WidgetHolder>().Use<WidgetHolder>();
            });
            
            container.Model.For<WidgetHolder>().Default.Instance.DependsOn(dependencyType)
                .ShouldBe(isDependent);
        }
        
        public class WidgetHolder
        {
            public WidgetHolder(IWidget widget)
            {
            }
        }
    }
}