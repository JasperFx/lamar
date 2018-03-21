using System.Threading.Tasks;
using Xunit;

namespace Lamar.Testing
{
    public class Container_Warmup_Tests
    {
        [Fact]
        public async Task warmup_first_time()
        {
            await Container.Warmup();
        }
        
        [Fact]
        public async Task warmup_multiple_times()
        {
            await Container.Warmup();
            await Container.Warmup();
            await Container.Warmup();
            await Container.Warmup();
            await Container.Warmup();
            await Container.Warmup();
            await Container.Warmup();
            
        }
    }
}