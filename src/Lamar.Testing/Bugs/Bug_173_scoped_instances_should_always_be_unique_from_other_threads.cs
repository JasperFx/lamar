using Shouldly;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Lamar.Testing.Bugs
{
    public class Bug_173_scoped_instances_should_always_be_unique_from_other_threads
    {
        public interface ITransientWidget
        {
            Guid GetGuid();
        }

        public interface IScopedWidget
        {
            Guid GetGuid();
        }

        public class TransientWidget : ITransientWidget
        {
            private readonly IScopedWidget widget;

            public TransientWidget(IScopedWidget widget)
            {
                this.widget = widget;
            }

            public Guid GetGuid()
            {
                return this.widget.GetGuid();
            }
        }

        public class ScopedWidget : IScopedWidget
        {
            private Guid id = Guid.NewGuid();

            public Guid GetGuid()
            {
                return this.id;
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(50)]
        public void scoped_owened_by_transient_is_unique(int mdop)
        {
            const int numberToGenerate = 100000;

            Container container = new Container(_ =>
            {
                _.For<ITransientWidget>().Use<TransientWidget>().Transient();
                _.For<IScopedWidget>().Use<ScopedWidget>().Scoped();
            });

            ConcurrentQueue<Guid> produce = new ConcurrentQueue<Guid>();

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = mdop
            };

            Parallel.For(0, numberToGenerate, parallelOptions, (iteration) =>
            {
                using (var scope = container.GetNestedContainer())
                {
                    produce.Enqueue(scope.GetInstance<ITransientWidget>().GetGuid());
                }
            });

            produce
                .Distinct()
                .Count()
                .ShouldBe(numberToGenerate);
        }
    }
}
