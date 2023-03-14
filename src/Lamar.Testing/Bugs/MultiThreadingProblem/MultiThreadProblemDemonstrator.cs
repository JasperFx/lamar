using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Lamar.Testing.Bugs.MultiThreadingProblem;

public class MultiThreadProblemDemonstrator
{
    private readonly ITestOutputHelper _output;

    public MultiThreadProblemDemonstrator(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void do_not_blow_up()
    {
        var registry = new ServiceRegistry();

        registry.AddTransient<ISimpleAdapter, SimpleAdapterOne>();
        registry.AddTransient<ISimpleAdapter, SimpleAdapterTwo>();
        registry.AddTransient<ISimpleAdapter, SimpleAdapterThree>();
        registry.AddTransient<ISimpleAdapter, SimpleAdapterFour>();
        registry.AddTransient<ISimpleAdapter, SimpleAdapterFive>();
        registry.AddTransient<ImportMultiple1, ImportMultiple1>();
        registry.AddTransient<ImportMultiple2, ImportMultiple2>();
        registry.AddTransient<ImportMultiple3, ImportMultiple3>();


        var container = new Container(registry);


        var resolve = () =>
        {
            for (var i = 0; i < 1000000; i++)
            {
                var importMultiple1 = (ImportMultiple1)container.GetService(typeof(ImportMultiple1));
                var importMultiple2 = (ImportMultiple2)container.GetService(typeof(ImportMultiple2));
                var importMultiple3 = (ImportMultiple3)container.GetService(typeof(ImportMultiple3));
            }
        };

        _output.WriteLine("########################### Single thread ###########################");

        var task0 = new Task(resolve);
        task0.Start();
        Task.WaitAll(task0);

        _output.WriteLine("########################### Two threads ###########################");

        var task1 = new Task(resolve);
        var task2 = new Task(resolve);

        task1.Start();
        task2.Start();

        Task.WaitAll(task1, task2);
    }
}