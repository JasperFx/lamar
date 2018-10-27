<!--Title: WhatDoIHave()-->

The `IContainer.WhatDoIHave()` method can give you a quick textual report of the current configuration of a running `Container`:

<[sample:whatdoihave-simple]>

Enough talk, say you have a `Container` with this configuration:

<[sample:what_do_i_have_container]>

If you were to run the code below against this `Container`:

<[sample:whatdoihave_everything]>

you would get the output shown in <a href="https://gist.github.com/jeremydmiller/7eae90eda21cc47ed24fa30623f9feb2" target="_new">this gist</a>.


If you're curious, all the raw code for this example is in [here](https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Diagnostics/WhatDoIHave_smoke_tests.cs).

## Filtering WhatDoIHave()

Filtering the `WhatDoIHave()` results can be done in these ways:

<[sample:whatdoihave-filtering]>

## WhatDoIHave() under ASP.Net Core

You can call `WhatDoIHave()` and `WhatDidIScan()` when running in ASP.Net Core like so:

<[sample:whatdoihave-aspnetcore]>
