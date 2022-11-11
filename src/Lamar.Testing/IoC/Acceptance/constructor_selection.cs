using Shouldly;
using StructureMap.Testing.Widget;
using StructureMap.Testing.Widget3;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    #region sample_custom-ctor-scenario
    public abstract class BaseThing
    {
        public BaseThing(IWidget widget)
        {
            CorrectCtorWasUsed = true;
        }

        public bool CorrectCtorWasUsed { get; set; }

        public BaseThing(IWidget widget, IService service)
        {
            Assert.True(false, "I should not have been called");
        }
    }

    public class Thing1 : BaseThing
    {
        public Thing1(IWidget widget) : base(widget)
        {
        }

        public Thing1(IWidget widget, IService service) : base(widget, service)
        {
        }
    }

    public class Thing2 : BaseThing
    {
        public Thing2(IWidget widget) : base(widget)
        {
        }

        public Thing2(IWidget widget, IService service) : base(widget, service)
        {
        }
    }

    #endregion



    public class constructor_selection
    {


        #region sample_using-default-ctor-attribute
        public class AttributedThing
        {
            // Normally the greediest ctor would be
            // selected, but using this attribute
            // will overrid that behavior
            [DefaultConstructor]
            public AttributedThing(IWidget widget)
            {
                CorrectCtorWasUsed = true;
            }

            public bool CorrectCtorWasUsed { get; set; }

            public AttributedThing(IWidget widget, IService service)
            {
                Assert.True(false, "I should not have been called");
            }
        }

        [Fact]
        public void select_constructor_by_attribute()
        {
            var container = new Container(_ => { _.For<IWidget>().Use<AWidget>(); });

            container.GetInstance<AttributedThing>()
                .CorrectCtorWasUsed
                .ShouldBeTrue();
        }

        #endregion

        #region sample_explicit-ctor-selection
        public class Thingie
        {
            public Thingie(IWidget widget)
            {
                CorrectCtorWasUsed = true;
            }

            public bool CorrectCtorWasUsed { get; set; }

            public Thingie(IWidget widget, IService service)
            {
                Assert.True(false, "I should not have been called");
            }
        }

        [Fact]
        public void override_the_constructor_selection()
        {
            var container = new Container(_ =>
            {
                _.For<IWidget>().Use<AWidget>();

                _.ForConcreteType<Thingie>().Configure

                    // StructureMap parses the expression passed
                    // into the method below to determine the
                    // constructor
                    .SelectConstructor(() => new Thingie(null));
            });

            container.GetInstance<Thingie>()
                .CorrectCtorWasUsed
                .ShouldBeTrue();
        }

        #endregion

        #region sample_skip-ctor-with-missing-simples
        public class DbContext
        {
            public string ConnectionString { get; set; }

            public DbContext(string connectionString)
            {
                ConnectionString = connectionString;
            }

            public DbContext() : this("default value")
            {
            }
        }

        [Fact]
        public void should_bypass_ctor_with_unresolvable_simple_args()
        {
            var container = Container.Empty();
            container.GetInstance<DbContext>()
                .ConnectionString.ShouldBe("default value");
        }

        [Fact]
        public void should_use_greediest_ctor_that_has_all_of_simple_dependencies()
        {
            var container = new Container(_ =>
            {
                _.ForConcreteType<DbContext>().Configure
                    .Ctor<string>("connectionString").Is("not the default");
            });

            container.GetInstance<DbContext>()
                .ConnectionString.ShouldBe("not the default");
        }

        #endregion
    }
}