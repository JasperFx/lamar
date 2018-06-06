<!--title:Injected Fields-->

There's a special kind of <[linkto:documentation/compilation/frames/variables;title=Variable]> called `InjectedField` that can be used to:

* Declare a private field within a generated type
* Establish a `Variable` that points to that private field
* Set up a constructor parameter for that field
* In the constructor, map the constructor parameter to the private field


As an example, let's take the `WhatTimeIsIt` generated type from the <[linkto:documentation/compilation/frames;title=frames model tutorial]>, but
this time generate the class with the assumption that the "now" time is injected into the generated type's constructor
like this:

<[sample:using-injected-field]>

At runtime as Lamar tries to write the code for a new generated type, it will seek out any or all `InjectedField` variables
used within any of the methods and use those to generate a constructor function. The generated code for the dynamic type
built up above will end up looking like this:

```
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
