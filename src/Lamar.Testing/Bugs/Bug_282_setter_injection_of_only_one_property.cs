using Shouldly;
using Xunit;

namespace Lamar.Testing.Bugs
{
    public class Bug_282_setter_injection_of_only_one_property
    {
        public interface IThingWithProperties
        {
            object PropertyA { get; set; }
            object PropertyB { get; set; }
            object PropertyC { get; set; }
        }

        public class ThingWithProperties : IThingWithProperties
        {
            public object PropertyA { get; set; } = "A";
            public object PropertyB { get; set; } = "B";
            public object PropertyC { get; set; } = "C";
        }

        [Fact]
        public void should_only_set_one_value()
        {
            var container = new Container(x =>
            {
                x.For<IThingWithProperties>().Use<ThingWithProperties>()
                    .Setter<object>(nameof(IThingWithProperties.PropertyB)).Is("X");
            });

            var thing = container.GetInstance<IThingWithProperties>();
            
            thing.PropertyB.ShouldBe("X");
            
            thing.PropertyA.ShouldBe("A");
            thing.PropertyC.ShouldBe("C");
        }
    }
}