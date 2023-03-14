using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance;

public class build_by_lambdas
{
    [Fact]
    public void build_with_lambda_that_gets_from_context()
    {
        var container = new Container(x =>
        {
            x.For<Rule>().Add(c => c.GetInstance<inline_dependencies.RuleBuilder>().ForColor("Beige"));
        });

        container.GetInstance<Rule>().ShouldBeOfType<ColorRule>().Color.ShouldBe("Beige");
    }

    #region sample_build-with-lambdas

    [Fact]
    public void build_with_lambdas_1()
    {
        var container = new Container(x =>
        {
            // Build by a static instance
            x.For<Rule>().Add(new ColorRule("Red")).Named("Red");

            // Build by Func<T> with a user supplied description
            x.For<Rule>()
                .Add(s => s.GetInstance<inline_dependencies.RuleBuilder>().ForColor("Green"))
                .Named("Green");

            // Build by Func<IBuildSession, T> with a user description
            x.For<Rule>()
                .Add(s => s.GetInstance<inline_dependencies.RuleBuilder>().ForColor("Purple"))
                .Named("Purple");
        });

        container.GetInstance<Rule>("Red").ShouldBeOfType<ColorRule>().Color.ShouldBe("Red");
        container.GetInstance<Rule>("Green").ShouldBeOfType<ColorRule>().Color.ShouldBe("Green");
        container.GetInstance<Rule>("Purple").ShouldBeOfType<ColorRule>().Color.ShouldBe("Purple");
    }

    #endregion

    #region sample_lambdas-as-inline-dependency

    [Fact]
    public void as_inline_dependency()
    {
        var container = new Container(x =>
        {
            // Build by a simple Expression<Func<T>>
            x.For<inline_dependencies.RuleHolder>()
                .Add<inline_dependencies.RuleHolder>()
                .Named("Red")
                .Ctor<Rule>()
                .Is(new ColorRule("Red"));

            // Build by a simple Expression<Func<IBuildSession, T>>
            x.For<inline_dependencies.RuleHolder>()
                .Add<inline_dependencies.RuleHolder>()
                .Named("Blue")
                .Ctor<Rule>()
                .Is("The Blue One", _ => { return new ColorRule("Blue"); });

            // Build by Func<T> with a user supplied description
            x.For<inline_dependencies.RuleHolder>()
                .Add<inline_dependencies.RuleHolder>()
                .Named("Green")
                .Ctor<Rule>()
                .Is("The Green One", s => s.GetInstance<inline_dependencies.RuleBuilder>().ForColor("Green"));

            // Build by Func<IBuildSession, T> with a user description
            x.For<inline_dependencies.RuleHolder>()
                .Add<inline_dependencies.RuleHolder>()
                .Named("Purple")
                .Ctor<Rule>()
                .Is("The Purple One",
                    s => { return s.GetInstance<inline_dependencies.RuleBuilder>().ForColor("Purple"); });
        });

        container.GetInstance<inline_dependencies.RuleHolder>("Red").Rule.ShouldBeOfType<ColorRule>().Color
            .ShouldBe("Red");
        container.GetInstance<inline_dependencies.RuleHolder>("Blue").Rule.ShouldBeOfType<ColorRule>().Color
            .ShouldBe("Blue");
        container.GetInstance<inline_dependencies.RuleHolder>("Green").Rule.ShouldBeOfType<ColorRule>().Color
            .ShouldBe("Green");
        container.GetInstance<inline_dependencies.RuleHolder>("Purple").Rule.ShouldBeOfType<ColorRule>().Color
            .ShouldBe("Purple");
    }

    #endregion
}