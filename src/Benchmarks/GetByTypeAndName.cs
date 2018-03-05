using BenchmarkDotNet.Attributes;
using Lamar;

namespace Benchmarks
{
    public class GetByTypeAndName : BenchmarkBase
    {
        public readonly string[] Names = new string[]{"Red", "Blue", "Purple", "Orange", "Yellow", "Green", "Violet"};
        
        protected override void configure(ServiceRegistry services)
        {
            foreach (var name in Names)
            {
                services.For<string>().Use(name).Named(name);
            }
        }

        // Really just testing the data structures here
        [Benchmark]
        public void fetch_by_type_and_name()
        {
            foreach (var name in Names)
            {
                var value = Lamar.GetInstance<string>(name);
            }
            
            
        }
    }
}