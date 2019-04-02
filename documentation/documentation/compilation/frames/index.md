<!--title:The "Frame" Model-->


The purpose of the "frames" model is to be able to generate dynamic methods by declaring a list of logical operations in generated code via Frame objects, then let Lamar handle:

* Finding any missing dependencies of those frames
* Determine what the necessary variable inputs are
* Ordering all the frames based on dependency order just prior to generating the code

Before diving into an example, here's a little class diagram of the main types:

<[img:content/images/LamarCodeGenClassDiagram.png;The Code Generation Model]>

The various types represent:

* `Frame` - Named after the StackFrame objects within stack traces in .Net. Models a logical action done in the generated code. Concrete examples in Lamar or Jasper would be calling a method on an object, calling a constructor function, or specific frame objects to create wrapped transaction boundaries or exception handling boundaries.
* `Variable` - pretty well what it sounds like. This type models a variable within the generated method, but also includes information about what Frame created it to help order the dependencies
* `IVariableSource` - mechanism to "find" or create variables. Examples in Lamar include resolving a service from an IoC container, passing along a method argument, or the example below that tells you the current time
* `IMethodVariables` - interface that is used by Frame classes to go find their necessary Variable dependencies.

Alrighty then, let's make this concrete. Let's say that we want to generate and use dynamic instances of this interface:

<[sample:ISaySomething]>

Moreover, I want a version of `ISaySomething` that will call the following method and write the current time to the console:

<[sample:NowSpeaker]>

Our dynamic class for ISaySomething will need to pass the current time to the now parameter of that method. To help out here, there's some built in helpers in Lamar specifically to write in the right code to get the current time to a variable of DateTime or DateTimeOffset that is named "now."

To skip ahead a little bit, let's generate a new class and object with the following code:

<[sample:write-new-method]>

After all that, if we interrogate the source code for the generated type above (type.SourceCode), we'd see this ugly generated code:

```
    public class WhatTimeIsIt : Lamar.Testing.Samples.ISaySomething
    {


        public void Speak()
        {
            var now = System.DateTime.UtcNow;
            Lamar.Testing.Samples.NowSpeaker.Speak(now);
        }

    }
```

Some notes about the generated code:

* Lamar was able to stick in some additional code to pass the current time into a new variable, and call the Speak(DateTime now) method with that value.
* Lamar is smart enough to put that code before the call to the method (that kind of matters here)
* The generated code uses full type names in almost all cases to avoid type collisions rather than trying to get smart with using statements in the generated code

So now let's look at how Lamar was able to add the code to pass along DateTime.UtcNow. First off, let's look at the code that just writes out the date variable:

<[sample:NowFetchFrame]>

In the frame above, you'll see that the `GenerateCode()` method writes its code into the source, then immediately turns around and tells the next Frame - if there is one - to generated its code. As the last step to write out the new source code, Lamar:

1. Goes through an effort to find any missing frames and variables
1. Sorts them with a topological sort (what frames depend on what other frames or variables, what variables are used or created by what frames)
1. Organizes the frames into a single linked list
1. Calls `GenerateCode()` on the first frame

In the generated method up above, the call to `NowSpeaker.Speak(now)` depends on the now variable which is in turn created by the `NowFetchFrame`, and that's enough information for Lamar to order things and generate the final code.

Lastly, we had to use a custom `IVariableSource` to teach Lamar how to resolve the now variable. That code looks like this:

<[sample:NowTimeVariableSource]>

Out of the box, the Lamar + [Jasper](https://jasperfx.github.io) combination uses variable sources for:

* Services from the internal IoC container of the application
* Method arguments
* Variables that can be derived from a method argument like `HttpContext.Request : HttpRequest`
* The "now" convention shown here

## GeneratedAssembly/Type/Method

Getting a little deeper into the parts of the "frames" model, see this class diagram:

<[img:content/images/GeneratedAssemblyModel.png;GeneratedAssembly Model]>


For more information, see:

<[TableOfContents]>