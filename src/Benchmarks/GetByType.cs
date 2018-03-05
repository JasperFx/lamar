using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Benchmarks
{
    public class GetByType : ComparisonBenchmark
    {
        [Benchmark]
        public void CreateScope()
        {
            var provider = For(ProviderName);
            var scope = provider.GetService<IServiceScopeFactory>().CreateScope();
        }
        

        
        
        /*
         
        [Benchmark]
        public void Lambdas()
        {
            buildAll(lambdas);
        }
        
        [Benchmark]
        public void Internals()
        {
            buildAll(internals);
        }
         
        [Benchmark]
        public void Objects()
        {
            buildAll(objects);
        }
         
        [Benchmark]
        public void Singletons()
        {
            buildAll(singletons);
        }

        [Benchmark]
        public void Scope()
        {
            buildAll(scoped);
        }
        
        [Benchmark]
        public void Transients()
        {
            buildAll(transients);
        }
        
        

        
                

        */
    }
}