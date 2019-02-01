using System;
using System.Linq;
using LamarCompiler;
using Xunit;
using Xunit.Abstractions;

namespace Lamar.Testing.Samples
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

        [Fact]
        public void say_hello()
        {
            var generator = new AssemblyGenerator();
            generator.ReferenceAssembly(typeof(IGreeter).Assembly);
            
            var assembly = generator.Generate(w =>
            {
                w.Write(@"


public class HelloWorld : Lamar.Testing.Samples.IGreeter
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




    }
    
}