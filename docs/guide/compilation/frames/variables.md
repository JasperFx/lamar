# Working with Variables

The `Variable` class in LamarCodeGeneration models the CLR type and usage of a value within a generated method.

Here are some samples of creating variable objects with a variable type and name:

<[sample:create-a-variable]>

## Default Naming

If you do not give a name for a variable, Lamar will derive a default variable name from the type like this:

<[sample:default-variable-name-usage]>

The best way to understand the full rules for deriving the default variable names is to just peek at the
[unit tests within the Lamar codebase](https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Codegen/VariableTests.cs).

## Creator Frame

If a variable is created by a [Frame](/guide/compilation/frames/frame), you really want to mark that relationship by
either passing the creating frame into the constructor function like this:

<[sample:NowFetchFrame]>

## Overriding Variable Usage or Type

Do this sparingly, but you can override the name or usage and type of a previously built variable like this:

<[sample:override-variable-usage-and-type]>

## Derived Variables

Variables don't have to mean literal C# variables in the generated code. They can be derived values like this example:

<[sample:derived-variable]>

## Dependencies to Other Variables

For the sake of frame ordering, you might need to give Lamar a hint that your variable depends on another variable. Here's
an example of doing that with the `HttpResponse` type from ASP.Net Core:

<[sample:variable-dependencies]>
