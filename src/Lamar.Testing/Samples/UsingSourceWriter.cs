using System;
using LamarCompiler;
using Xunit;
using Xunit.Abstractions;

namespace Lamar.Testing.Samples
{
    public class UsingSourceWriter
    {
        private readonly ITestOutputHelper _output;

        public UsingSourceWriter(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void write_code()
        {
            // SAMPLE: simple-usage-of-source-writer
            var writer = new SourceWriter();
            
            writer.Write(@"
BLOCK:public void SayHello()
Console.WriteLine('Hello');
END
".Replace("'", "\""));
            
            Console.WriteLine(writer.Code());
            // ENDSAMPLE
            
            _output.WriteLine(writer.Code());
        }

        public void other_methods()
        {
            // SAMPLE: other-sourcewriter-basics
            var writer = new SourceWriter();
            
            // Write an empty line into the code 
            writer.BlankLine();
            
            // Writes a single line into the code
            // with the proper indention. Does NOT
            // respect the BLOCK: and END directives
            writer.WriteLine("// A comment");
            
            // Writes a closing '}' character into the 
            // next line and decrements the leading space
            // indention for the following lines of code
            writer.FinishBlock();
            // ENDSAMPLE
        }


        public void advanced_usages()
        {
            // SAMPLE: SourceWriterAdvanced
            var writer = new SourceWriter();
            
            // Add "using [namespace]; statements
            writer.UsingNamespace(typeof(Console).Namespace);
            writer.UsingNamespace<IOperation>();
            
            writer.Namespace("GeneratedCode");
            // Write new classes and code within the namespace
            writer.FinishBlock();
            
            
            // Helper to write using blocks in C# code
            writer.UsingBlock("var conn = new SqlConnection()", w =>
            {
                w.Write("conn.Open();");
                // other statements
            });
            
            
            
            // Write a comment text into the code at the correct indention
            // level
            writer.WriteComment("Some message");
            
            
            // Start the declaration of a new public class named "MyClass"
            // that implements the IDisposable interface
            writer.StartClass("MyClass", typeof(IDisposable));
            // ENDSAMPLE
        }
    }
}