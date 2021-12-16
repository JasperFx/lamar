using System;
using System.Threading.Tasks;
using Baseline;
using Lamar;
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
            var generator = host.Services.GetRequiredService<DynamicCodeBuilder>();

            generator.Rules.TypeLoadMode = TypeLoadMode.Static;



            foreach (var generatesCode in generator.Generators)
            {
                foreach (var file in generatesCode.BuildFiles())
                {
                    await file.Initialize(generator.Rules, generatesCode, generator.Services);
                }
            }
            

            return true;
        }
    }
}