﻿using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;

namespace StructureMap.Testing.Acceptance
{
    public class build_by_lambdas
    {
        #region sample_build-with-lambdas
        [Fact]
        public void build_with_lambdas_1()
        {
            var container = new Container(x =>
            {
                // Build by a simple Expression<Func<T>>
                x.For<Rule>().Add(() => new ColorRule("Red")).Named("Red");

                // Build by a simple Expression<Func<IBuildSession, T>>
                x.For<Rule>().Add("Blue", () => new ColorRule("Blue"))
                    .Named("Blue");

                // Build by Func<T> with a user supplied description
                x.For<Rule>()
                    .Add(s => s.GetInstance<RuleBuilder>().ForColor("Green"))
                    .Named("Green");

                // Build by Func<IBuildSession, T> with a user description
                x.For<Rule>()
                    .Add("Purple", s => s.GetInstance<RuleBuilder>().ForColor("Purple"))
                    .Named("Purple");
            });

            container.GetInstance<Rule>("Red").ShouldBeOfType<ColorRule>().Color.ShouldBe("Red");
            container.GetInstance<Rule>("Blue").ShouldBeOfType<ColorRule>().Color.ShouldBe("Blue");
            container.GetInstance<Rule>("Green").ShouldBeOfType<ColorRule>().Color.ShouldBe("Green");
            container.GetInstance<Rule>("Purple").ShouldBeOfType<ColorRule>().Color.ShouldBe("Purple");
        }

        #endregion

        #region sample_build-with-lambdas-in-batch
        [Fact]
        public void add_batch_of_lambdas()
        {
            var container = new Container(x =>
            {
                x.For<Rule>().AddInstances(rules =>
                {
                    // Build by a simple Expression<Func<T>>
                    rules.ConstructedBy(() => new ColorRule("Red")).Named("Red");

                    // Build by a simple Expression<Func<IBuildSession, T>>
                    rules.ConstructedBy("Blue", () => { return new ColorRule("Blue"); }).Named("Blue");

                    // Build by Func<T> with a user supplied description
                    rules
                        .ConstructedBy(s => s.GetInstance<RuleBuilder>().ForColor("Green"))
                        .Named("Green");

                    // Build by Func<IBuildSession, T> with a user description
                    rules.ConstructedBy("Purple", s => { return s.GetInstance<RuleBuilder>().ForColor("Purple"); })
                        .Named("Purple");
                });
            });

            container.GetInstance<Rule>("Red").ShouldBeOfType<ColorRule>().Color.ShouldBe("Red");
            container.GetInstance<Rule>("Blue").ShouldBeOfType<ColorRule>().Color.ShouldBe("Blue");
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
                x.For<RuleHolder>()
                    .Add<RuleHolder>()
                    .Named("Red")
                    .Ctor<Rule>().Is(() => new ColorRule("Red"));

                // Build by a simple Expression<Func<IBuildSession, T>>
                x.For<RuleHolder>()
                    .Add<RuleHolder>()
                    .Named("Blue").
                    Ctor<Rule>().Is("The Blue One", () => { return new ColorRule("Blue"); });

                // Build by Func<T> with a user supplied description
                x.For<RuleHolder>()
                    .Add<RuleHolder>()
                    .Named("Green")
                    .Ctor<Rule>().Is("The Green One", s => s.GetInstance<RuleBuilder>().ForColor("Green"));

                // Build by Func<IBuildSession, T> with a user description
                x.For<RuleHolder>()
                    .Add<RuleHolder>()
                    .Named("Purple")
                    .Ctor<Rule>()
                    .Is("The Purple One", s => { return s.GetInstance<RuleBuilder>().ForColor("Purple"); });
            });

            container.GetInstance<RuleHolder>("Red").Rule.ShouldBeOfType<ColorRule>().Color.ShouldBe("Red");
            container.GetInstance<RuleHolder>("Blue").Rule.ShouldBeOfType<ColorRule>().Color.ShouldBe("Blue");
            container.GetInstance<RuleHolder>("Green").Rule.ShouldBeOfType<ColorRule>().Color.ShouldBe("Green");
            container.GetInstance<RuleHolder>("Purple").Rule.ShouldBeOfType<ColorRule>().Color.ShouldBe("Purple");
        }

        #endregion
    }

    #region sample_RuleHolder
    public class RuleHolder
    {
        private readonly Rule _rule;

        public RuleHolder(Rule rule)
        {
            _rule = rule;
        }

        public Rule Rule
        {
            get { return _rule; }
        }
    }

    #endregion

    #region sample_RuleBuilder
    public class RuleBuilder
    {
        public Rule ForColor(string color)
        {
            return new ColorRule(color);
        }
    }

    #endregion
}