﻿using Shouldly;
using StructureMap.Pipeline;
using Xunit;

namespace StructureMap.Testing.Acceptance
{
    public class explicit_arguments
    {
        [Fact]
        public void supply_defaults_by_generic()
        {
            #region sample_explicit-arg-container
            var container = new Container(x =>
            {
                x.For<IWidget>().Use<BWidget>();
                x.For<IService>().Use<AService>();
            });
            #endregion

            #region sample_explicit-fluent-interface
            var widget = new BWidget();
            var service = new BService();

            var guyWithWidgetAndService = container
                .With<IWidget>(widget)
                .With<IService>(service)
                .GetInstance<GuyWithWidgetAndService>();

            guyWithWidgetAndService
                .Widget.ShouldBeTheSameAs(widget);

            guyWithWidgetAndService
                .Service.ShouldBeTheSameAs(service);
            #endregion
        }

        [Fact]
        public void supply_defaults_with_args()
        {
            var container = new Container(x =>
            {
                x.For<IWidget>().Use<BWidget>();
                x.For<IService>().Use<AService>();
            });

            #region sample_explicit-use-explicit-args
            var widget = new BWidget();
            var service = new BService();

            var args = new ExplicitArguments();
            args.Set<IWidget>(widget);
            args.Set<IService>(service);

            var guyWithWidgetAndService = container
                .GetInstance<GuyWithWidgetAndService>(args);

            guyWithWidgetAndService
                .Widget.ShouldBeTheSameAs(widget);

            guyWithWidgetAndService
                .Service.ShouldBeTheSameAs(service);
            #endregion
        }

        [Fact]
        public void supply_defaults_by_generic_in_a_bunch()
        {
            var container = new Container(x =>
            {
                x.For<IWidget>().Use<BWidget>();
                x.For<IService>().Use<AService>();
            });

            #region sample_explicit-defaults-with-nested-closure
            var widget = new BWidget();
            var service = new BService();

            var guyWithWidgetAndService = container
                .With(x =>
                {
                    x.With<IWidget>(widget);
                    x.With<IService>(service);
                })
                .GetInstance<GuyWithWidgetAndService>();

            guyWithWidgetAndService
                .Widget.ShouldBeTheSameAs(widget);

            guyWithWidgetAndService
                .Service.ShouldBeTheSameAs(service);
            #endregion
        }

        #region sample_explicit-named-arguments
        [Fact]
        public void supply_named_arguments()
        {
            var container = new Container(x => { x.For<IWidget>().Use<ColorWidget>().Ctor<string>().Is("Red"); });

            container.GetInstance<IWidget>()
                .ShouldBeOfType<ColorWidget>()
                .Color.ShouldBe("Red");

            container.With("color").EqualTo("Blue")
                .GetInstance<IWidget>()
                .ShouldBeOfType<ColorWidget>()
                .Color.ShouldBe("Blue");
        }

        #endregion
    }

    #region sample_explicit-domain
    public class ColorWidget : IWidget
    {
        public string Color { get; set; }

        public ColorWidget(string color)
        {
            Color = color;
        }
    }

    public class GuyWithWidgetAndService
    {
        public IWidget Widget { get; set; }
        public IService Service { get; set; }

        public GuyWithWidgetAndService(IWidget widget, IService service)
        {
            Widget = widget;
            Service = service;
        }
    }

    #endregion
}