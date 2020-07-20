using System.Threading;
using System.Threading.Tasks;
using Lamar.IoC.Instances;
using Shouldly;
using StructureMap.Testing.Widget;
using Xunit;
using LamarCodeGeneration.Util;

namespace Lamar.Testing.IoC
{
    public class compiling_lambdas
    {
        [Fact]
        public void generate_no_arg_constructor()
        {
            var container = Container.For(x => x.For<IWidget>().Use<AWidget>());

            var func = container.Model.For<IWidget>().Default.Instance.As<GeneratedInstance>()
                .BuildFuncResolver(container);

            func(container).ShouldBeOfType<AWidget>();
        }

        [Fact]
        public void inject_constructor_arguments()
        {
            var container = Container.For(x =>
            {
                x.For<IWidget>().Use<AWidget>();
                x.For<Rule>().Use<ARule>();
                x.For<BigGuy>().Use<BigGuy>();
            });

            var func = container.Model.For<BigGuy>().Default.Instance.As<GeneratedInstance>()
                .BuildFuncResolver(container);

            var guy = func(container).ShouldBeOfType<BigGuy>();

            guy.Widget.ShouldBeOfType<AWidget>();
            guy.Rule.ShouldBeOfType<ARule>();
        }
        
        [Fact]
        public void inject_constructor_arguments_using_a_lambda()
        {
            var container = Container.For(x =>
            {
                x.For<IWidget>().Use(c => new AWidget());
                x.For<Rule>().Use<ARule>();
                x.For<BigGuy>().Use<BigGuy>();
            });

            var func = container.Model.For<BigGuy>().Default.Instance.As<GeneratedInstance>()
                .BuildFuncResolver(container);

            var guy = func(container).ShouldBeOfType<BigGuy>();

            guy.Widget.ShouldBeOfType<AWidget>();
            guy.Rule.ShouldBeOfType<ARule>();
        }

        [Fact]
        public void inject_setter_with_concrete()
        {
            var container = Container.For(x =>
            {
                x.For<IWidget>().Use<AWidget>();
                x.For<Rule>().Use<ARule>();
                x.For<BigGuy>().Use<BigGuy>().Setter<IServer>().Is<NulloServer>();
            });

            var func = container.Model.For<BigGuy>().Default.Instance.As<GeneratedInstance>()
                .BuildFuncResolver(container);

            var guy = func(container).ShouldBeOfType<BigGuy>();

            guy.Server.ShouldBeOfType<NulloServer>();
        }
        
        [Fact]
        public void inject_setter_with_lambda()
        {
            var container = Container.For(x =>
            {
                x.For<IWidget>().Use<AWidget>();
                x.For<Rule>().Use<ARule>();
                x.For<BigGuy>().Use<BigGuy>().Setter<IServer>().Is(c => new NulloServer());
            });

            var func = container.Model.For<BigGuy>().Default.Instance.As<GeneratedInstance>()
                .BuildFuncResolver(container);

            var guy = func(container).ShouldBeOfType<BigGuy>();

            guy.Server.ShouldBeOfType<NulloServer>();
        }
        
        [Fact]
        public void inject_ctor_arg_with_object()
        {
            var rule = new ARule();
            
            var container = Container.For(x =>
            {
                x.For<IWidget>().Use<AWidget>();
                x.For<Rule>().Use(rule);
                x.For<BigGuy>().Use<BigGuy>().Setter<IServer>().Is<NulloServer>();
            });

            var func = container.Model.For<BigGuy>().Default.Instance.As<GeneratedInstance>()
                .BuildFuncResolver(container);

            var guy = func(container).ShouldBeOfType<BigGuy>();

            guy.Rule.ShouldBeSameAs(rule);
        }

        [Fact]
        public void inject_setter_with_object()
        {
            var server = new NulloServer();
            
            var container = Container.For(x =>
            {
                x.For<IWidget>().Use<AWidget>();
                x.For<Rule>().Use<ARule>();
                x.For<BigGuy>().Use<BigGuy>().Setter<IServer>().Is(server);
            });

            var func = container.Model.For<BigGuy>().Default.Instance.As<GeneratedInstance>()
                .BuildFuncResolver(container);

            var guy = func(container).ShouldBeOfType<BigGuy>();
            
            guy.Server.ShouldBeSameAs(server);
        }
        
        [Fact]
        public void inject_setter_with_primitive()
        {
            
            var container = Container.For(x =>
            {
                x.For<IWidget>().Use<AWidget>();
                x.For<Rule>().Use<ARule>();
                x.For<BigGuy>().Use<BigGuy>().Setter<int>().Is(11);
            });

            var func = container.Model.For<BigGuy>().Default.Instance.As<GeneratedInstance>()
                .BuildFuncResolver(container);

            var guy = func(container).ShouldBeOfType<BigGuy>();
            
            guy.Number.ShouldBe(11);
        }
    }


    public interface IServer{}
    


    public class NulloServer : IServer
    {
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

    }
    
    public class BigGuy
    {
        public IWidget Widget { get; }
        public Rule Rule { get; }
        
        public int Number { get; set; }

        public BigGuy(IWidget widget, Rule rule)
        {
            Widget = widget;
            Rule = rule;
        }
        
        public IServer Server { get; set; }
    }
}