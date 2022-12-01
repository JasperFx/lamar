using System.Threading.Tasks;
using LamarCodeGeneration;
using LamarCompiler;
using Microsoft.Extensions.DependencyInjection;
using Oakton;

namespace GeneratorTarget
{
    public class WriteCommand : OaktonAsyncCommand<NetCoreInput>
    {
        public override async Task<bool> Execute(NetCoreInput input)
        {
            using var host = input.BuildHost();
            var collections = host.Services.GetServices<ICodeFileCollection>();

            foreach (var collection in collections)
            {
                foreach (var file in collection.BuildFiles())
                {
                    collection.Rules.TypeLoadMode = TypeLoadMode.Dynamic;
                    await file.Initialize(collection.Rules, collection, host.Services);
                }
            }
            

            return true;
        }
    }
}