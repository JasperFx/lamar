<!--title:Generating Code with ISourceWriter-->

This code was originally written and proven out in the related [Marten](https://jasperfx.github.io/marten) and described in a 
post titled [Using Roslyn for Runtime Code Generation in Marten](https://jeremydmiller.com/2015/11/11/using-roslyn-for-runtime-code-generation-in-marten/).
This code was ripped out of Marten itself, but it's happily running now in Lamar a couple years later.

Lamar provides the `Lamar.Compilation.ISourceWriter` service -- and a lot of related extension methods -- to help write common code constructs and 
maintain legible code indention just like you'd use if you were writing the code in an editor or IDE. 

## The Basics

To dip our toes into source generation, let's write a simple method to a string that would just write out "Hello" to the console:

<[sample:simple-usage-of-source-writer]>

After that code, the value of the `SourceWriter.Code()` method is this text:

```
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

<[sample:other-sourcewriter-basics]>

## Advanced Usages

<[warning]>
All the usages in this section are from extension methods in the `Lamar.Compilation` namespace
<[/warning]>

Here are some of the advanced usages of `ISourceWriter`:

<[sample:SourceWriterAdvanced]>
