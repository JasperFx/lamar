using Baseline;
using Lamar;
using LamarCodeGeneration;
using LamarCompiler;
using Microsoft.Extensions.DependencyInjection;
using Oakton;
using Oakton.AspNetCore;

namespace GeneratorTarget
{
    public class WriteCommand : OaktonCommand<NetCoreInput>
    {
        public override bool Execute(NetCoreInput input)
        {
            using (var host = input.BuildHost())
            {
                var generator = host.Services.GetRequiredService<DynamicCodeBuilder>();
                generator.TryBuildAndCompileAll((a, s) => new AssemblyGenerator().Compile(a, s));
                generator.AttachAllCompiledTypes(host.Services);

                var writers = host.Services.As<IContainer>().Model.GetAllPossible<ConsoleWriterGenerator>();

                foreach (var writer in writers)
                {
                    writer.WriteToConsole();
                }
            }

            return true;
        }
    }
}