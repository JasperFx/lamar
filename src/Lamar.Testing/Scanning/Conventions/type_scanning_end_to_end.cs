using System;
using System.Linq;
using System.Threading.Tasks;
using Lamar.Scanning.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.Scanning.Conventions
{
    public class type_scanning_end_to_end
    {
        public interface IFinder<T>
        {
        }

        public class StringFinder : IFinder<string>
        {
        }

        public class IntFinder : IFinder<int>
        {
        }

        public class DoubleFinder : IFinder<double>
        {
        }

        public interface IFindHandler<T>
        {
        }

        public class SrpViolation : IFinder<decimal>, IFindHandler<DateTime>
        {
        }

        public class SuperFinder : IFinder<byte>, IFinder<char>, IFinder<uint>
        {
        }

        [Fact]
        public void can_configure_plugin_families_via_dsl()
        {
            var container = new Container(registry => registry.Scan(x =>
            {
                x.TheCallingAssembly();
                x.ConnectImplementationsToTypesClosing(typeof(IFinder<>));
            }));


            var firstStringFinder = container.GetInstance<IFinder<string>>().ShouldBeOfType<StringFinder>();
            var secondStringFinder = container.GetInstance<IFinder<string>>().ShouldBeOfType<StringFinder>();

            var firstIntFinder = container.GetInstance<IFinder<int>>().ShouldBeOfType<IntFinder>();
            var secondIntFinder = container.GetInstance<IFinder<int>>().ShouldBeOfType<IntFinder>();
        }

        [Fact]
        public void can_find_the_closed_finders()
        {
            var container = new Container(x => x.Scan(o =>
            {
                o.TheCallingAssembly();
                o.ConnectImplementationsToTypesClosing(typeof(IFinder<>));
            }));
            
            container.GetInstance<IFinder<string>>().ShouldBeOfType<StringFinder>();
            container.GetInstance<IFinder<int>>().ShouldBeOfType<IntFinder>();
            container.GetInstance<IFinder<double>>().ShouldBeOfType<DoubleFinder>();
        }

        [Theory]
        [InlineData(ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped)]
        public void connect_implementations_with_lifetime(ServiceLifetime lifetime)
        {
            var container = new Container(x => x.Scan(o =>
            {
                o.TheCallingAssembly();
                o.ConnectImplementationsToTypesClosing(typeof(IFinder<>), lifetime);
            }));

            container.Model.For<IFinder<string>>().Default.Lifetime.ShouldBe(lifetime);
            container.Model.For<IFinder<int>>().Default.Lifetime.ShouldBe(lifetime);
            container.Model.For<IFinder<double>>().Default.Lifetime.ShouldBe(lifetime);
        }

        [Fact]
        public void fails_on_closed_type()
        {
            Exception<InvalidOperationException>.ShouldBeThrownBy(
                () => { new GenericConnectionScanner(typeof(double)); });
        }

        [Fact]
        public void find_all_implementations()
        {
            var container = Container.For(_ =>
            {
                 _.Scan(x =>
                 {
                     x.AssemblyContainingType<IShoes>();
                     x.AssemblyContainingType<IWidget>();
                     x.AddAllTypesOf<IWidget>();
                 });   
            });

            var widgetTypes = container.Model.For<IWidget>()
                .Instances.Select(x => x.ImplementationType).ToArray();

            widgetTypes.ShouldContain(typeof(MoneyWidget));
            widgetTypes.ShouldContain(typeof(AWidget));
        }

        [Fact]
        public void single_class_can_close_multiple_open_interfaces()
        {
            var container = new Container(x => x.Scan(o =>
            {
                o.TheCallingAssembly();
                o.ConnectImplementationsToTypesClosing(typeof(IFinder<>));
                o.ConnectImplementationsToTypesClosing(typeof(IFindHandler<>));
            }));
            container.GetInstance<IFinder<decimal>>().ShouldBeOfType<SrpViolation>();
            container.GetInstance<IFindHandler<DateTime>>().ShouldBeOfType<SrpViolation>();
        }

        [Fact]
        public void single_class_can_close_the_same_open_interface_multiple_times()
        {
            var container = new Container(x => x.Scan(o =>
            {
                o.TheCallingAssembly();
                o.ConnectImplementationsToTypesClosing(typeof(IFinder<>));
            }));
            container.GetInstance<IFinder<byte>>().ShouldBeOfType<SuperFinder>();
            container.GetInstance<IFinder<char>>().ShouldBeOfType<SuperFinder>();
            container.GetInstance<IFinder<uint>>().ShouldBeOfType<SuperFinder>();
        }

        [Fact]
        public void single_implementation()
        {
            var container = Container.For(_ =>
            {
                _.Scan(x =>
                {
                    x.AssemblyContainingType<IShoes>();
                    x.SingleImplementationsOfInterface();
                });
            });

            container.Model.For<Muppet>().Default.ImplementationType.ShouldBe(typeof(Grover));
        }

        [Theory]
        [InlineData(ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped)]
        public void single_implementation_with_lifetime(ServiceLifetime lifetime)
        {
            var container = Container.For(_ =>
            {
                _.Scan(x =>
                {
                    x.AssemblyContainingType<IShoes>();
                    x.SingleImplementationsOfInterface(lifetime);
                });
            });

            container.Model.For<Muppet>().Default.Lifetime.ShouldBe(lifetime);
        }

        [Fact]
        public void use_default_scanning()
        {
            var container = Container.For(_ =>
            {
                _.Scan(x =>
                {
                    x.AssemblyContainingType<IShoes>();
                    x.WithDefaultConventions();
                });
                
            });

            container.Model.For<IShoes>().Default.ImplementationType.ShouldBe(typeof(Shoes));
            container.Model.For<IShorts>().Default.ImplementationType.ShouldBe(typeof(Shorts));
        }

        [Theory]
        [InlineData(ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped)]
        public void use_default_scanning_with_lifetime(ServiceLifetime lifetime)
        {
            var container = Container.For(_ =>
            {
                _.Scan(x =>
                {
                    x.AssemblyContainingType<IShoes>();
                    x.WithDefaultConventions(lifetime);
                });

            });

            container.Model.For<IShoes>().Default.Lifetime.ShouldBe(lifetime);
            container.Model.For<IShorts>().Default.Lifetime.ShouldBe(lifetime);
        }

        [Fact]
        public void use_default_scanning_with_no_overrides()
        {
            var container = Container.For(_ =>
            {
                _.For<IRanger>().Use<RedRanger>();

                _.Scan(x =>
                {
                    x.AssemblyContainingType<IRanger>();
                    x.WithDefaultConventions(OverwriteBehavior.Never);
                });
            });
            
            container.Model.For<IRanger>().Instances.Single()
                .ImplementationType.ShouldBe(typeof(RedRanger));
        }
        
        [Fact]
        public void use_default_scanning_with_if_newer_and_not_a_match()
        {
            var container = Container.For(_ =>
            {
                _.For<IRanger>().Use<RedRanger>();

                _.Scan(x =>
                {
                    x.AssemblyContainingType<IRanger>();
                    x.WithDefaultConventions(OverwriteBehavior.NewType);
                });
            });
            
            container.Model.For<IRanger>().Instances.Select(x => x.ImplementationType)
                .ShouldHaveTheSameElementsAs(typeof(RedRanger), typeof(Ranger));
        }
        
        [Fact]
        public void use_default_scanning_with_if_newer_and_a_match()
        {
            var container = Container.For(_ =>
            {
                _.For<IRanger>().Use<Ranger>();

                _.Scan(x =>
                {
                    x.AssemblyContainingType<IRanger>();
                    x.WithDefaultConventions(OverwriteBehavior.NewType);
                });
            });
            
            container.Model.For<IRanger>().Instances.Single()
                .ImplementationType.ShouldBe(typeof(Ranger));
        }

    }
    
    public interface IRanger{}
    public class Ranger : IRanger {}
    public class RedRanger : IRanger{}


    public interface Muppet
    {
    }

    public class Grover : Muppet
    {
    }

    public interface IShoes
    {
    }

    public class Shoes : IShoes
    {
    }

    public interface IShorts
    {
    }

    public class Shorts : IShorts
    {
    }
    
        public class SingleImplementationScannerTester
    {
        private readonly Container container;

        public SingleImplementationScannerTester()
        {
            container = new Container(registry => registry.Scan(x =>
            {
                x.TheCallingAssembly();
                x.IncludeNamespaceContainingType<SingleImplementationScannerTester>();
                x.SingleImplementationsOfInterface();
            }));
        }

        [Fact]
        public void registers_plugins_that_only_have_a_single_implementation()
        {
            container.GetInstance<IOnlyHaveASingleConcreteImplementation>()
                .ShouldBeOfType<MyNameIsNotConventionallyRelatedToMyInterface>();
        }

        [Theory]
        [InlineData(ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped)]
        public void single_implementation_with_lifetime(ServiceLifetime lifetime)
        {
            var container = new Container(registry => registry.Scan(x =>
            {
                x.TheCallingAssembly();
                x.IncludeNamespaceContainingType<SingleImplementationScannerTester>();
                x.SingleImplementationsOfInterface(lifetime);
            }));

            container.Model.For<IOnlyHaveASingleConcreteImplementation>().Default.Lifetime.ShouldBe(lifetime);
        }

    }

    public interface IOnlyHaveASingleConcreteImplementation
    {
    }

    public class MyNameIsNotConventionallyRelatedToMyInterface : IOnlyHaveASingleConcreteImplementation
    {
    }

    public interface IHaveMultipleConcreteImplementations
    {
    }

    public class FirstConcreteImplementation : IHaveMultipleConcreteImplementations
    {
    }

    public class SecondConcreteImplementation : IHaveMultipleConcreteImplementations
    {
    }
    
    
    
        public class TypeFindingTester
    {
        public TypeFindingTester()
        {
            container = new Container(registry =>
            {
                registry.For<INormalType>();
                registry.Scan(x =>
                {
                    x.TheCallingAssembly();
                    x.AddAllTypesOf<TypeIWantToFind>();
                    x.AddAllTypesOf<OtherType>();
                });
            });
        }

        private readonly IContainer container;

        [Fact]
        public void FoundTheRightNumberOfInstancesForATypeWithNoPlugins()
        {
            container.GetAllInstances<TypeIWantToFind>().Count
                .ShouldBe(3);
        }

        [Fact]
        public void FoundTheRightNumberOfInstancesForATypeWithNoPlugins2()
        {
            container.GetAllInstances<OtherType>().Count.ShouldBe(2);
        }

        [Theory]
        [InlineData(ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped)]
        public void add_all_types_with_lifetime(ServiceLifetime lifetime)
        {
            var container = new Container(registry =>
            {
                registry.For<INormalType>();
                registry.Scan(x =>
                {
                    x.TheCallingAssembly();
                    x.AddAllTypesOf<TypeIWantToFind>(lifetime);
                    x.AddAllTypesOf<OtherType>(lifetime);
                });
            });

            container.Model.For<TypeIWantToFind>().Default.Lifetime.ShouldBe(lifetime);
            container.Model.For<OtherType>().Default.Lifetime.ShouldBe(lifetime);
        }

        public interface IOpenGeneric<T>
        {
            void Nop();
        }

        public interface IAnotherOpenGeneric<T>
        {
        }

        public class ConcreteOpenGeneric<T> : IOpenGeneric<T>
        {
            public void Nop()
            {
            }
        }

        public class StringOpenGeneric : ConcreteOpenGeneric<string>
        {
        }

        public class when_finding_all_types_implementing_and_open_generic_interface
        {
            [Fact]
            public void it_can_find_all_implementations()
            {
                using (var container = new Container(c => c.Scan(s =>
                {
                    s.AddAllTypesOf(typeof(IOpenGeneric<>));
                    s.TheCallingAssembly();
                })))
                {
                    var redTypes = container.GetAllInstances<IOpenGeneric<string>>();

                    redTypes.Count.ShouldBe(1);
                }
            }

            [Fact]
            public void it_can_override_generic_implementation_with_specific()
            {
                var container = new Container(c => c.Scan(s =>
                {
                    s.AddAllTypesOf(typeof(IOpenGeneric<>));
                    s.TheCallingAssembly();
                }));

                using (container)
                {
                    var redType = container.GetInstance<IOpenGeneric<string>>();
                    redType.ShouldBeOfType<StringOpenGeneric>();
                }
            }
        }
    }

    public interface TypeIWantToFind
    {
    }

    public class RedType
    {
    }

    public class BlueType : TypeIWantToFind
    {
    }

    public class PurpleType : TypeIWantToFind
    {
    }

    public class YellowType : TypeIWantToFind
    {
    }

    public class GreenType : TypeIWantToFind
    {
        private GreenType()
        {
        }
    }

    public abstract class OrangeType : TypeIWantToFind
    {
    }

    public class OtherType
    {
    }

    public class DifferentOtherType : OtherType
    {
    }

    public interface INormalType
    {
    }

    //[Pluggable("First")]
    public class NormalTypeWithPluggableAttribute : INormalType
    {
    }

    public class SecondNormalType : INormalType
    {
    }
    
    
}