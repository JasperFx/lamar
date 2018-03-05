using Lamar.IoC.Instances;
using Shouldly;
using StructureMap.Testing.GenericWidgets;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Instances
{
    public class deriving_default_arg_name
    {
        [Fact]
        public void name_when_unique()
        {
            var instance = ConstructorInstance.For<AWidget>();
            instance.IsOnlyOneOfServiceType = true;
            
            instance.DefaultArgName().ShouldBe("aWidget");
        }

        [Fact]
        public void name_for_closed_generic_type_that_is_only_one()
        {
            var instance = ConstructorInstance.For<Service<IWidget>>();
            instance.IsOnlyOneOfServiceType = true;
            
            instance.DefaultArgName().ShouldBe("service_of_IWidget");
        }
    }
}