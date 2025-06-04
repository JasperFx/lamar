using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Lamar.Testing.Bugs
{
    public class Bug_413_allow_user_defined_null_value
    {
        public interface IThing
        {
        }

        public interface IThingConsumer
        {
            IThing Thing { get; }
        }

        public class ThingConsumer : IThingConsumer
        {
            public IThing Thing { get; private set; }
            public ThingConsumer(IThing thing)
            {
                this.Thing = thing;
            }
        }

        [Fact]
        public void ShouldGetNullDependency_WhenDependencyResolvesToNullSingleton()
        {
            var container = new Container(x =>
            {
                x.For<IThing>().Use(null as IThing);//.Singleton();
                x.For<IThingConsumer>().Use<ThingConsumer>();
            });

            var theThing = container.GetInstance<IThing>();

            //This blows up when IThing is defined as null singleton/user defined variable in the registry
            //Expression of type 'System.Object' cannot be used for constructor parameter of type 'typename+IThing' (Parameter 'arguments[0]')'
            var thingConsumerByItsRegistration = container.GetInstance<IThingConsumer>();
            var thingConsumerByConcrete = container.GetInstance<ThingConsumer>();

            theThing.ShouldBeNull();
            thingConsumerByItsRegistration.Thing.ShouldBeNull();
            thingConsumerByItsRegistration.ShouldBeOfType<ThingConsumer>();
            thingConsumerByConcrete.Thing.ShouldBeNull();
            thingConsumerByConcrete.ShouldBeOfType<ThingConsumer>();
        }
    }
}
