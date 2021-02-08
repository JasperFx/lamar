using System.Threading.Tasks;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using Shouldly;
using Xunit;

namespace LamarCompiler.Testing.Codegen.Methods
{
    public class AsyncSingleReturnGeneratedMethodTester
    {
        [Fact]
        public async Task can_generate_method()
        {
            var assembly = new GeneratedAssembly(new GenerationRules("Jasper.Generated"));

            
            var generatedType = assembly.AddType("NumberGetter", typeof(INumberGetter));
            
            generatedType.MethodFor("GetNumber").Frames.Append(new ReturnFive());
            
            assembly.CompileAll();

            var getter = generatedType.CreateInstance<INumberGetter>();

            var number = await getter.GetNumber();
            
            number.ShouldBe(5);
        }
    }

    public class ReturnFive : AsyncFrame
    {
        public override bool CanReturnTask()
        {
            return true;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"return {typeof(Task).FullNameInCode()}.FromResult(5);");
        }
    }

    public interface INumberGetter
    {
        Task<int> GetNumber();
    }
}