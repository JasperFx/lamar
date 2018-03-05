using System.Diagnostics;
using System.Linq;
using Shouldly;
using Xunit;

namespace Lamar.Testing.IoC.Acceptance
{
    public class generic_types
    {
        // SAMPLE: register_open_generic_type
        [Fact]
        public void register_open_generic_type()
        {
            var container = new Container(_ =>
            {
                _.For(typeof(IVisualizer<>)).Use(typeof(DefaultVisualizer<>));
            });


            container.GetInstance<IVisualizer<IssueCreated>>()
                .ShouldBeOfType<DefaultVisualizer<IssueCreated>>();


            container.GetInstance<IVisualizer<IssueResolved>>()
                .ShouldBeOfType<DefaultVisualizer<IssueResolved>>();
        }

        // ENDSAMPLE

        [Fact]
        public void using_visualizer()
        {
            // SAMPLE: using-visualizer-knowning-the-type
            // Just setting up a Container and ILogVisualizer
            var container = Container.For<VisualizationRegistry>();
            var visualizer = container.GetInstance<ILogVisualizer>();

            // If I have an IssueCreated lob object...
            var created = new IssueCreated();

            // I can get the html representation:
            var html = visualizer.ToHtml(created);
            // ENDSAMPLE
        }

        [Fact]
        public void a_bunch_of_logs()
        {
            // SAMPLE: using-visualizer-not-knowing-the-type
            var logs = new object[]
            {
                new IssueCreated(),
                new TaskAssigned(),
                new Comment(),
                new IssueResolved()
            };

            // SAMPLE: using-visualizer-knowning-the-type
            // Just setting up a Container and ILogVisualizer
            var container = Container.For<VisualizationRegistry>();
            var visualizer = container.GetInstance<ILogVisualizer>();

            var items = logs.Select(visualizer.ToHtml);
            var html = string.Join("<hr />", items);
            // ENDSAMPLE
        }

        // SAMPLE: generic-defaults-with-fallback
        [Fact]
        public void generic_defaults()
        {
            var container = new Container(_ =>
            {
                // The default visualizer just like we did above
                _.For(typeof(IVisualizer<>)).Use(typeof(DefaultVisualizer<>));

                // Register a specific visualizer for IssueCreated
                _.For<IVisualizer<IssueCreated>>().Use<IssueCreatedVisualizer>();
            });

            // We have a specific visualizer for IssueCreated
            container.GetInstance<IVisualizer<IssueCreated>>()
                .ShouldBeOfType<IssueCreatedVisualizer>();

            // We do not have any special visualizer for TaskAssigned,
            // so fall back to the DefaultVisualizer<T>
            container.GetInstance<IVisualizer<TaskAssigned>>()
                .ShouldBeOfType<DefaultVisualizer<TaskAssigned>>();
        }

        // ENDSAMPLE

        // SAMPLE: visualization-registry-in-action
        [Fact]
        public void visualization_registry()
        {
            var container = Container.For<VisualizationRegistry>();


            container.GetInstance<IVisualizer<IssueCreated>>()
                .ShouldBeOfType<IssueCreatedVisualizer>();

            container.GetInstance<IVisualizer<IssueResolved>>()
                .ShouldBeOfType<IssueResolvedVisualizer>();

            // We have no special registration for TaskAssigned,
            // so fallback to the default visualizer
            container.GetInstance<IVisualizer<TaskAssigned>>()
                .ShouldBeOfType<DefaultVisualizer<TaskAssigned>>();
        }

        // ENDSAMPLE
    }
        // SAMPLE: VisualizationRegistry
    public class VisualizationRegistry : ServiceRegistry
    {
        public VisualizationRegistry()
        {
            // The main ILogVisualizer service
            For<ILogVisualizer>().Use<LogVisualizer>();

            // A default, fallback visualizer
            For(typeof(IVisualizer<>)).Use(typeof(DefaultVisualizer<>));

            // Auto-register all concrete types that "close"
            // IVisualizer<TLog>
            Scan(x =>
            {
                x.TheCallingAssembly();
                x.ConnectImplementationsToTypesClosing(typeof(IVisualizer<>));
            });

        }
    }
    // ENDSAMPLE

    // SAMPLE: IVisualizer<T>
    public interface IVisualizer<TLog>
    {
        string ToHtml(TLog log);
    }
    // ENDSAMPLE

    // SAMPLE: ILogVisualizer
    public interface ILogVisualizer
    {
        // If we already know what the type of log we have
        string ToHtml<TLog>(TLog log);

        // If we only know that we have a log object
        string ToHtml(object log);
    }
    // ENDSAMPLE

    // SAMPLE: DefaultVisualizer
    public class DefaultVisualizer<TLog> : IVisualizer<TLog>
    {
        public string ToHtml(TLog log)
        {
            return string.Format("<div>{0}</div>", log);
        }
    }
    // ENDSAMPLE


    public class SpecialLog { }

    public class TypicalLog { }
    public class TaskAssigned { }
    public class IssueResolved { }
    public class Comment { }


    public class IssueCreated { }

    // SAMPLE: specific-visualizers
    public class IssueCreatedVisualizer : IVisualizer<IssueCreated>
    {
        public string ToHtml(IssueCreated log)
        {
            return "special html for an issue being created";
        }
    }

    public class IssueResolvedVisualizer : IVisualizer<IssueResolved>
    {
        public string ToHtml(IssueResolved log)
        {
            return "special html for issue resolved";
        }
    }
    // ENDSAMPLE


    public class Visualizer
    {
        private readonly IContainer _container;

        public Visualizer(IContainer container)
        {
            _container = container;
        }

        // SAMPLE: to-html-already-knowning-the-log-type
        public string ToHtml<TLog>(TLog log)
        {
            // _container is a reference to an IContainer object
            return _container.GetInstance<IVisualizer<TLog>>().ToHtml(log);
        }
        // ENDSAMPLE
    }

    // SAMPLE: LogVisualizer
    public class LogVisualizer : ILogVisualizer
    {
        private readonly IContainer _container;

        // Take in the IContainer directly so that
        // yes, you can use it as a service locator
        public LogVisualizer(IContainer container)
        {
            _container = container;
        }

        // It's easy if you already know what the log
        // type is
        public string ToHtml<TLog>(TLog log)
        {
            return _container.GetInstance<IVisualizer<TLog>>()
                .ToHtml(log);
        }

        public string ToHtml(object log)
        {
            return null;
        }

        public string ToHtml2(object log)
        {
            return null;
        }

        // The IWriter and Writer<T> class below are
        // adapters to go from "object" to <T>() signatures
        public interface IWriter
        {
            string Write(object log);
        }

        public class Writer<T> : IWriter
        {
            private readonly IVisualizer<T> _visualizer;

            public Writer(IVisualizer<T> visualizer)
            {
                _visualizer = visualizer;
            }

            public string Write(object log)
            {
                return _visualizer.ToHtml((T) log);
            }
        }
    }
}