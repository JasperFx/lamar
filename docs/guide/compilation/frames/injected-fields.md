# Injected Fields

There's a special kind of [Variable](/guide/compilation/frames/variables) called `InjectedField` that can be used to:

* Declare a private field within a generated type
* Establish a `Variable` that points to that private field
* Set up a constructor parameter for that field
* In the constructor, map the constructor parameter to the private field

As an example, let's take the `WhatTimeIsIt` generated type from the [frames model tutorial](/guide/compilation/frames), but
this time generate the class with the assumption that the "now" time is injected into the generated type's constructor
like this:

<!-- snippet: sample_using-injected-field -->
<a id='snippet-sample_using-injected-field'></a>
```cs
var assembly = GeneratedAssembly.Empty();
var type = assembly.AddType("WhatTimeIsIt", typeof(ISaySomething));

var method = type.MethodFor(nameof(ISaySomething.Speak));

var @call = new MethodCall(typeof(NowSpeaker), nameof(NowSpeaker.Speak));

// Create an InjectedField as the argument to
// the Speak method
var now = new InjectedField(typeof(DateTime), "now");
@call.Arguments[0] = now;

method.Frames.Add(@call);

assembly.CompileAll();
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/LamarCompiler.Testing/Samples/InjectedFieldUsage.cs#L22-L38' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using-injected-field' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

At runtime as Lamar tries to write the code for a new generated type, it will seek out any or all `InjectedField` variables
used within any of the methods and use those to generate a constructor function. The generated code for the dynamic type
built up above will end up looking like this:

```csharp
public class WhatTimeIsIt : Lamar.Testing.Samples.ISaySomething
{
    private readonly DateTime _now;

    public WhatTimeIsIt(DateTime now)
    {
        _now = now;
    }

    public void Speak()
    {
        Lamar.Testing.Samples.NowSpeaker.Speak(_now);
    }
}
```
