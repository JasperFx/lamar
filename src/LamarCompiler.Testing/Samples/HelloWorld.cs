using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace LamarCompiler.Testing.Samples
{
    public interface IGreeter
    {
        string Greetings();
    }


    public class HelloWorldSamples
    {
        private readonly ITestOutputHelper _output;

        public HelloWorldSamples(ITestOutputHelper output)
        {
            _output = output;
        }
#if !NET4x

        [Fact]
        public void say_hello()
        {
            var generator = new AssemblyGenerator();
            generator.ReferenceAssembly(typeof(IGreeter).Assembly);
            
            var assembly = generator.Generate(w =>
            {
                w.Write(@"


public class HelloWorld : LamarCompiler.Testing.Samples.IGreeter
{
    public string Greetings()
    {
        return ""Hello NDC London!"";
    }
}
");
            });
            
            var greeter = (IGreeter)Activator.CreateInstance(assembly.GetExportedTypes().Single());
            
            
            _output.WriteLine(greeter.Greetings());
            
        }
#endif




    }
    
}