using System.Threading.Tasks;
using Lamar.Codegen;
using Lamar.Codegen.Frames;
using Lamar.Compilation;
using Shouldly;
using Xunit;

namespace Lamar.Testing.Codegen.Methods
{
    public class AsyncSingleReturnGeneratedMethodTester
    {
        [Fact]
        public async Task can_generate_method()
        {
            var assembly = new GeneratedAssembly(new GenerationRules("Jasper.Generated"));

            
            var generatedType = assembly.AddType("NumberGetter", typeof(INumberGetter));
            
            generatedType.MethodFor("GetNumber").Add(new ReturnFive());
            
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
            writer.Write("return Task.FromResult(5);");
        }
    }

    public interface INumberGetter
    {
        Task<int> GetNumber();
    }
}