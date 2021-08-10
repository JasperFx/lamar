using StructureMap.Graph;
using StructureMap.Testing.Widget3;
using StructureMap.TypeRules;
using System.Linq;
using Xunit;

namespace StructureMap.Testing.Graph
{
    public class FirstInterfaceConventionTester
    {
        public FirstInterfaceConventionTester()
        {
            #region sample_using-RegisterConcreteTypesAgainstTheFirstInterface
            container = new Container(x =>
            {
                x.Scan(o =>
                {
                    o.TheCallingAssembly();
                    o.RegisterConcreteTypesAgainstTheFirstInterface();

                    o.Exclude(t => t.CanBeCastTo(typeof(IGateway)));
                });
            });
            #endregion
        }

        private Container container;

        public interface I1
        {
        }

        public interface I2
        {
        }

        public interface I3
        {
        }

        public class C1 : I1
        {
        }

        public class C2 : C1, I2
        {
        }

        public class C3 : C2, I3
        {
            public C3(string name)
            {
            }
        }

        public class C4
        {
        }

        public class C5 : I1
        {
            private C5()
            {
            }
        }

        public interface I1<T>
        {
        }

        public interface I2<T>
        {
        }

        public interface I3<T>
        {
        }

        public class C1<T> : I1<T>
        {
        }

        public class C2<T> : C1<T>, I2<T>
        {
        }

        public class C3<T> : C2<T>, I3<T>
        {
        }

        [Fact]
        public void do_not_register_type_if_there_are_primitive_arguments()
        {
            container.Model.HasImplementationsFor<I3>().ShouldBeFalse();
        }

        [Fact]
        public void simple_case()
        {
            container.Model.For<I1>().Instances.Any(x => x.ReturnedType == typeof(C1)).ShouldBeTrue();
            container.Model.For<I1>().Instances.Any(x => x.ReturnedType == typeof(C2)).ShouldBeTrue();

            container.Model.For<I2>().Instances.Select(x => x.ReturnedType).Any().ShouldBeFalse();
        }
    }
}