using System;
using System.Linq;
using Lamar.Compilation;
using Lamar.Testing.Codegen;
using Shouldly;
using Xunit;

namespace Lamar.Testing.Samples
{
    // SAMPLE: IOperation
    public interface IOperation
    {
        int Calculate(int one, int two);
    }
    // ENDSAMPLE

    public class AddOperator : IOperation
    {
        public int Calculate(int one, int two)
        {
            return one + two;
        }
    }
    
    public class Codegen
    {
        [Fact]
        public void generate_code_on_the_fly()
        {
            // SAMPLE: using-AssemblyGenerator
            var generator = new AssemblyGenerator();

            // This is necessary for the compilation to succeed
            // It's exactly the equivalent of adding references
            // to your project
            generator.ReferenceAssembly(typeof(Console).Assembly);
            generator.ReferenceAssembly(typeof(IOperation).Assembly);

            // Compile and generate a new .Net Assembly object
            // in memory
            var assembly = generator.Generate(@"
using Lamar.Testing.Samples;

namespace Generated
{
    public class AddOperator : IOperation
    {
        public int Calculate(int one, int two)
        {
            return one + two;
        }
    }
}
");

            // Find the new type we generated up above
            var type = assembly.GetExportedTypes().Single();
            
            // Use Activator.CreateInstance() to build an object
            // instance of our new class, and cast it to the 
            // IOperation interface
            var operation = (IOperation)Activator.CreateInstance(type);

            // Use our new type
            var result = operation.Calculate(1, 2);
            
            // ENDSAMPLE
            result.ShouldBe(3);
        }

        [Fact]
        public void generate_code_on_the_fly_using_source_writer()
        {
            // SAMPLE: using-AssemblyGenerator-with-source-writer
            var generator = new AssemblyGenerator();

            // This is necessary for the compilation to succeed
            // It's exactly the equivalent of adding references
            // to your project
            generator.ReferenceAssembly(typeof(Console).Assembly);
            generator.ReferenceAssembly(typeof(IOperation).Assembly);


            var assembly = generator.Generate(x =>
            {
                x.Namespace("Generated");
                x.StartClass("AddOperator", typeof(IOperation));
                
                x.Write("BLOCK:public int Calculate(int one, int two)");
                x.Write("return one + two;");
                x.FinishBlock();  // Finish the method
                
                x.FinishBlock();  // Finish the class
                x.FinishBlock();  // Finish the namespace
            });
            


            var type = assembly.GetExportedTypes().Single();
            var operation = (IOperation)Activator.CreateInstance(type);

            var result = operation.Calculate(1, 2);
            
            // ENDSAMPLE
            result.ShouldBe(3);
        }

        [Fact]
        public void generate_assembly_with_random_name_by_default()
        {
            var generator = new AssemblyGenerator();

            var assembly = generator.Generate("public class Given{}");

            assembly.GetName().Name.ShouldNotBeNullOrWhiteSpace();
        }

        [Fact]
        public void generate_assembly_with_given_name()
        {
            var generator = new AssemblyGenerator
            {
                AssemblyName = "given",
            };

            var assembly = generator.Generate("public class Given{}");

            assembly.GetName().Name.ShouldBe("given");
        }
    }
}