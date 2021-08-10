# Compiling Code with AssemblyGenerator

::: tip INFO
The Lamar team thinks most users will use the [Frames](/guide/compilation/frames/) to generate and compile code, but you
might very well wish to bypass that admittedly complicated model and just use the inner utility classes
that are shown in this page.
:::

If all you want to do is take some C# code and compile that in memory to a new, in memory assembly, you can use
the `LamarCompiler.AssemblyGenerator` class in the LamarCompiler library.

Let's say that you have a simple interface in your system like this:

<!-- snippet: sample_IOperation -->
<a id='snippet-sample_ioperation'></a>
```cs
public interface IOperation
{
    int Calculate(int one, int two);
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/LamarCompiler.Testing/Samples/Codegen.cs#L9-L14' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_ioperation' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Next, let's use `AssemblyGenerator` to compile code with a custom implementation of `IOperation` that we've generated
in code:

<!-- snippet: sample_using-AssemblyGenerator -->
<a id='snippet-sample_using-assemblygenerator'></a>
```cs
var generator = new AssemblyGenerator();

// This is necessary for the compilation to succeed
// It's exactly the equivalent of adding references
// to your project
generator.ReferenceAssembly(typeof(Console).Assembly);
generator.ReferenceAssembly(typeof(IOperation).Assembly);

// Compile and generate a new .Net Assembly object
// in memory
var assembly = generator.Generate(@"
using LamarCompiler.Testing.Samples;

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
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/LamarCompiler.Testing/Samples/Codegen.cs#L29-L66' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-assemblygenerator' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

There's only a couple things going on in the code above:

1. I added an assembly reference for the .Net assembly that holds the `IOperation` interface
1. I passed a string to the `GenerateCode()` method, which successfully compiles my code and hands me back a .Net [Assembly](https://msdn.microsoft.com/en-us/library/system.reflection.assembly(v=vs.110).aspx) object
1. Load the newly generated type from the new Assembly
1. Use the new `IOperation`

If you're not perfectly keen on doing brute force string manipulation to generate your code, you can
also use Lamar's built in [ISourceWriter](/guide/compilation/source-writer) to generate some of the code for you with
all its code generation utilities:

<!-- snippet: sample_using-AssemblyGenerator-with-source-writer -->
<a id='snippet-sample_using-assemblygenerator-with-source-writer'></a>
```cs
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
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/LamarCompiler.Testing/Samples/Codegen.cs#L73-L103' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-assemblygenerator-with-source-writer' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
