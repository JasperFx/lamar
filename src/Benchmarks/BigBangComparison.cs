using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    public class BigBangComparison : ComparisonBenchmark
    {
        [Benchmark]
        public void AllTypes()
        {
            buildAll(Types);
        }
    }
}