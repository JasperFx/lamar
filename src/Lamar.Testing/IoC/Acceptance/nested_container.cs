using System;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class nested_containers
    {
        // SAMPLE: nested-creation
        public interface IWorker
        {
            void DoWork();
        }

        public class Worker : IWorker, IDisposable
        {
            public void DoWork()
            {
                // do stuff!
            }

            public void Dispose()
            {
                // clean up
            }
        }

        [Fact]
        public void creating_a_nested_container()
        {
            // From an IContainer object
            var container = new Container(_ => { _.For<IWorker>().Use<Worker>(); });

            using (var nested = container.GetNestedContainer())
            {
                // This object is disposed when the nested container
                // is disposed
                var worker = nested.GetInstance<IWorker>();
                worker.DoWork();
            }
        }

        // ENDSAMPLE

        // SAMPLE: nested-singletons
        [Fact]
        public void nested_container_usage_of_singletons()
        {
            var container = new Container(_ =>
            {
                _.ForSingletonOf<IColorCache>().Use<ColorCache>();
            });

            var singleton = container.GetInstance<IColorCache>();

            // SingletonThing's are resolved from the parent container
            using (var nested = container.GetNestedContainer())
            {
                nested.GetInstance<IColorCache>()
                    .ShouldBeSameAs(singleton);
            }
        }

        // ENDSAMPLE

        // SAMPLE: nested-transients
        [Fact]
        public void nested_container_behavior_of_scoped()
        {
            // "Transient" is the default lifecycle
            // in StructureMap
            var container = new Container(_ =>
            {
                _.AddScoped<IColor, Green>();
            });

            // In a normal Container, a "scoped" lifecycle
            // Instance will be built up once for the root Container
            container.GetInstance<IColor>()
                .ShouldBeSameAs(container.GetInstance<IColor>());

            // From a nested container, the "scoped" lifecycle
            // is tracked to the nested container
            using (var nested = container.GetNestedContainer())
            {
                nested.GetInstance<IColor>()
                    .ShouldBeSameAs(nested.GetInstance<IColor>());

                // One more time
                nested.GetInstance<IColor>()
                    .ShouldBeSameAs(nested.GetInstance<IColor>());
            }
        }

        // ENDSAMPLE



        // SAMPLE: nested-disposal
        [Fact]
        public void nested_container_disposal()
        {
            var container = new Container(_ =>
            {
                // A SingletonThing scoped service
                _.ForSingletonOf<IColorCache>().Use<ColorCache>();

                // A transient scoped service
                _.For<IColor>().Use<Green>();

                
                
                // An AlwaysUnique scoped service
                _.AddTransient<Purple>();

                _.AddTransient<Blue>();
            });

            ColorCache singleton = null;
            Green nestedGreen = null;
            Blue nestedBlue = null;
            Purple nestedPurple = null;

            using (var nested = container.GetNestedContainer())
            {
                // SingletonThing's are really built by the parent
                singleton = nested.GetInstance<IColorCache>()
                    .ShouldBeOfType<ColorCache>();

                nestedGreen = nested.GetInstance<IColor>()
                    .ShouldBeOfType<Green>();

                nestedBlue = nested.GetInstance<Blue>();

                nestedPurple = nested.GetInstance<Purple>();
            }

            // Transients created by the Nested Container
            // are disposed
            nestedGreen.WasDisposed.ShouldBeTrue();
            nestedBlue.WasDisposed.ShouldBeTrue();

            // Unique's created by the Nested Container
            // are disposed
            nestedPurple.WasDisposed.ShouldBeTrue();

            // NOT disposed because it's owned by
            // the parent container
            singleton.WasDisposed.ShouldBeFalse();
        }

        // ENDSAMPLE


        // SAMPLE: nested-func-lazy-and-container-resolution
        public class Foo
        {
        }

        public class FooHolder
        {
            public IContainer Container { get; set; }
            public Func<Foo> Func { get; set; }
            public Lazy<Foo> Lazy { get; set; }

            public FooHolder(IContainer container, Func<Foo> func, Lazy<Foo> lazy)
            {
                Container = container;
                Func = func;
                Lazy = lazy;
            }
        }



        // ENDSAMPLE

        [Fact]
        public void always_unique_disposal()
        {
            var container = new Container(_ =>
            {
                _.AddTransient<Blue>();
            });

            Blue nestedBlue1;
            Blue nestedBlue2;
            using (var nested = container.GetNestedContainer())
            {
                nestedBlue1 = nested.GetInstance<Blue>();
                nestedBlue2 = nested.GetInstance<Blue>();

                nestedBlue1.ShouldNotBeTheSameAs(nestedBlue2);
            }

            nestedBlue1.WasDisposed.ShouldBeTrue();
            nestedBlue2.WasDisposed.ShouldBeTrue();
        }

    }

    // SAMPLE: nested-colors
    public interface IColor
    {
    }

    public class Red : IColor
    {
    }

    public class Purple : Blue { }

    public class Blue : IColor, IDisposable
    {
        public bool WasDisposed;

        public void Dispose()
        {
            WasDisposed = true;
        }
    }

    public class Green : IColor, IDisposable
    {
        public bool WasDisposed;

        public void Dispose()
        {
            WasDisposed = true;
        }
    }

    public interface IColorCache
    {
    }

    public class ColorCache : IColorCache, IDisposable
    {
        public bool WasDisposed;

        public void Dispose()
        {
            WasDisposed = true;
        }
    }

    // ENDSAMPLE

    // SAMPLE: nested-http
    public interface IHttpRequest
    {
    }

    public interface IHttpResponse
    {
    }

    public class HttpRequest : IHttpRequest
    {
    }

    public class HttpResponse : IHttpResponse
    {
    }

    public class StandInHttpRequest : IHttpRequest
    {
    }

    public class StandInHttpResponse : IHttpResponse
    {
    }

    public class HttpRequestHandler
    {
        private readonly IHttpRequest _request;
        private readonly IHttpResponse _response;

        public HttpRequestHandler(IHttpRequest request, IHttpResponse response)
        {
            _request = request;
            _response = response;
        }

        public IHttpRequest Request
        {
            get { return _request; }
        }

        public IHttpResponse Response
        {
            get { return _response; }
        }
    }

    // ENDSAMPLE

    public interface IUnitOfWork
    {
    }

    public class UnitOfWork : IUnitOfWork
    {
    }

    public class HandlerA
    {
        public HandlerA(IUnitOfWork uow)
        {
        }
    }

    public class HandlerB
    {
        public HandlerB(IUnitOfWork uow)
        {
        }
    }

    public class HandlerC
    {
        public HandlerC(IUnitOfWork uow)
        {
        }
    }
}