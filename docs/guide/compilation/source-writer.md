# Generating Code with ISourceWriter

::: tip INFO
As of Lamar v3.0, all of the code compilation is contained in the LamarCodeGeneration NuGet, and can be used independently of Lamar itself.
:::

This code was originally written and proven out in the related [Marten](https://jasperfx.github.io/marten) and described in a
post titled [Using Roslyn for Runtime Code Generation in Marten](https://jeremydmiller.com/2015/11/11/using-roslyn-for-runtime-code-generation-in-marten/).
This code was ripped out of Marten itself, but it's happily running now in Lamar a couple years later.

Lamar provides the `LamarCodeGeneration.ISourceWriter` service -- and a lot of related extension methods -- to help write common code constructs and
maintain legible code indention just like you'd use if you were writing the code in an editor or IDE.

## The Basics

To dip our toes into source generation, let's write a simple method to a string that would just write out "Hello" to the console:

<!-- snippet: sample_simple-usage-of-source-writer -->
<a id='snippet-sample_simple-usage-of-source-writer'></a>
```cs
var writer = new SourceWriter();

writer.Write(@"
BLOCK:public void SayHello()
Console.WriteLine('Hello');
END
".Replace("'", "\""));

Console.WriteLine(writer.Code());
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/LamarCompiler.Testing/Samples/UsingSourceWriter.cs#L20-L30' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_simple-usage-of-source-writer' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

After that code, the value of the `SourceWriter.Code()` method is this text:

```csharp
public void Go()
{
    Console.WriteLine("Hello");
}
```

A few notes on what `SourceWriter.Write()` is doing:

* Starting a line with _BLOCK:_ tells Lamar to write an open bracket '{' on the next line of code and to increment
  the leading spaces for subsequent lines
* The `Write()` method is processing each line in the text one at a time, so the call to `Console.WriteLine("Hello")` would be indented
  because it is inside a code block for the method
* The _END_ text tells Lamar to write a closing '}' bracket on the next line, then decrement the leading spaces for the next lines of code

Other basic method usages are shown below:

<!-- snippet: sample_other-sourcewriter-basics -->
<a id='snippet-sample_other-sourcewriter-basics'></a>
```cs
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
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/LamarCompiler.Testing/Samples/UsingSourceWriter.cs#L37-L52' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_other-sourcewriter-basics' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Advanced Usages

::: warning
All the usages in this section are from extension methods in the `Lamar.Compilation` namespace
:::

Here are some of the advanced usages of `ISourceWriter`:

<!-- snippet: sample_SourceWriterAdvanced -->
<a id='snippet-sample_sourcewriteradvanced'></a>
```cs
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
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/LamarCompiler.Testing/Samples/UsingSourceWriter.cs#L58-L87' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_sourcewriteradvanced' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
