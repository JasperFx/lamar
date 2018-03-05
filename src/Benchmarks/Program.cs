using BenchmarkDotNet.Running;

namespace Benchmarks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //var summary = BenchmarkRunner.Run<GetByType>();
            //var summary2 = BenchmarkRunner.Run<GetByTypeAndName>();
            var summary3 = BenchmarkRunner.Run<BigBangComparison>();
        }
    }
}