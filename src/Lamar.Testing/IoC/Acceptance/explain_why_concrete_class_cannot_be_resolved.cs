using Lamar.IoC;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class explain_why_concrete_class_cannot_be_resolved
    {
        [Fact]
        public void tells_you_the_reason_why_constructor_cannot_be_filled()
        {
            var container = Container.Empty();

            var ex = Should.Throw<LamarMissingRegistrationException>(() => { container.GetInstance<OtherWidgetHolder>(); });

            
            ex.Message.ShouldContain("Cannot fill the dependencies of any of the public constructors");
        }
        
        public class OtherWidgetHolder
        {
            public OtherWidgetHolder(IThing thing, IWidget widget) 
            {
            }
        }
    }
}