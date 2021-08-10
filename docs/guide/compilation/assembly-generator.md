# Compiling Code with AssemblyGenerator

::: tip INFO
The Lamar team thinks most users will use the [Frames](/guide/compilation/frames/) to generate and compile code, but you
might very well wish to bypass that admittedly complicated model and just use the inner utility classes
that are shown in this page.
:::

If all you want to do is take some C# code and compile that in memory to a new, in memory assembly, you can use
the `LamarCompiler.AssemblyGenerator` class in the LamarCompiler library.

Let's say that you have a simple interface in your system like this:

<[sample:IOperation]>

Next, let's use `AssemblyGenerator` to compile code with a custom implementation of `IOperation` that we've generated
in code:

<[sample:using-AssemblyGenerator]>

There's only a couple things going on in the code above:

1. I added an assembly reference for the .Net assembly that holds the `IOperation` interface
1. I passed a string to the `GenerateCode()` method, which successfully compiles my code and hands me back a .Net [Assembly](https://msdn.microsoft.com/en-us/library/system.reflection.assembly(v=vs.110).aspx) object
1. Load the newly generated type from the new Assembly
1. Use the new `IOperation`

If you're not perfectly keen on doing brute force string manipulation to generate your code, you can
also use Lamar's built in [ISourceWriter](/guide/compilation/source-writer) to generate some of the code for you with
all its code generation utilities:

<[sample:using-AssemblyGenerator-with-source-writer]>
