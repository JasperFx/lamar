# Working with Variables

The `Variable` class in LamarCodeGeneration models the CLR type and usage of a value within a generated method.

Here are some samples of creating variable objects with a variable type and name:

<!-- snippet: sample_create-a-variable -->
<a id='snippet-sample_create-a-variable'></a>
```cs
// Create a connection for the type SqlConnection 
// with the name "conn"
var conn = Variable.For<SqlConnection>("conn");

// Pretty well the same thing above
var conn2 = new Variable(typeof(SqlConnection), "conn2");

// Create a variable with the default name
// for the type
var conn3 = Variable.For<SqlConnection>();
conn3.Usage.ShouldBe("sqlConnection");
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/LamarCompiler.Testing/Samples/Variables.cs#L40-L52' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_create-a-variable' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Default Naming

If you do not give a name for a variable, Lamar will derive a default variable name from the type like this:

<!-- snippet: sample_default-variable-name-usage -->
<a id='snippet-sample_default-variable-name-usage'></a>
```cs
var widget = Variable.For<IWidget>();
widget.Usage.ShouldBe("widget");
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/LamarCompiler.Testing/Samples/Variables.cs#L26-L29' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_default-variable-name-usage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The best way to understand the full rules for deriving the default variable names is to just peek at the
[unit tests within the Lamar codebase](https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Codegen/VariableTests.cs).

## Creator Frame

If a variable is created by a [Frame](/guide/compilation/frames/frame), you really want to mark that relationship by
either passing the creating frame into the constructor function like this:

<!-- snippet: sample_NowFetchFrame -->
<a id='snippet-sample_nowfetchframe'></a>
```cs
public class NowFetchFrame : SyncFrame
{
    public NowFetchFrame(Type variableType)
    {
        // Notice how "this" frame is passed into the variable
        // class constructor as the creator
        Variable = new Variable(variableType, "now", this);
    }
    
    public Variable Variable { get; }
    
    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        writer.WriteLine($"var {Variable.Usage} = {Variable.VariableType.FullName}.{nameof(DateTime.UtcNow)};");
        Next?.GenerateCode(method, writer);
    }
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/LamarCodeGeneration/Model/NowTimeVariableSource.cs#L31-L49' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_nowfetchframe' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Overriding Variable Usage or Type

Do this sparingly, but you can override the name or usage and type of a previously built variable like this:

<!-- snippet: sample_override-variable-usage-and-type -->
<a id='snippet-sample_override-variable-usage-and-type'></a>
```cs
var service = new Variable(typeof(IService), "service");
service.OverrideName("myService");
service.OverrideType(typeof(WhateverService));
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/LamarCompiler.Testing/Samples/Variables.cs#L33-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_override-variable-usage-and-type' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Derived Variables

Variables don't have to mean literal C# variables in the generated code. They can be derived values like this example:

<!-- snippet: sample_derived-variable -->
<a id='snippet-sample_derived-variable'></a>
```cs
var now = new Variable(typeof(DateTime), $"{typeof(DateTime).FullName}.{nameof(DateTime.Now)}");
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/LamarCompiler.Testing/Samples/Variables.cs#L22-L24' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_derived-variable' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Dependencies to Other Variables

For the sake of frame ordering, you might need to give Lamar a hint that your variable depends on another variable. Here's
an example of doing that with the `HttpResponse` type from ASP.Net Core:

<!-- snippet: sample_variable-dependencies -->
<a id='snippet-sample_variable-dependencies'></a>
```cs
var context = Variable.For<HttpContext>();
var response = new Variable(typeof(HttpResponse), $"{context.Usage}.{nameof(HttpContext.Response)}");
response.Dependencies.Add(context);
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/LamarCompiler.Testing/Samples/Variables.cs#L56-L60' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_variable-dependencies' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
