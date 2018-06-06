<!--title:Extension Methods for Names in Code-->

To help generate code in memory, you'll want just a little bit of help from the following methods to
determine how a type should be written *in code* with these extension methods in `Lamar.Compilation`:

1. `Type.NameInCode()` -- gives you the type name as it should appear in code. Handles inner types, generic types, well known simple types like `int`, and all other types

1. `Type.FullNameInCode()` -- gives you the type name as it should appear in code. Handles inner types, generic types, well known simple types like `int`, and all other types


The functionality of `NameInCode()` is demonstrated below with some of its xUnit tests:

<[sample:get-the-type-name-in-code]>

Likewise, `FullNameInCode()` is shown below:

<[sample:get-the-full-type-name-in-code]>