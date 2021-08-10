# LamarCodeGeneration & LamarCompiler

Underneath Lamar the IoC container is a standalone library named `LamarCodeGeneration` that can be used by itself as a code generation and Lambda compilation tool. As of Lamar v3.0, there's also a separate library called `LamarCompiler` that is a tiny helper to do in memory compilation of C# via Roslyn. The [Jasper application framework](https://jasperfx.github.io)
heavily uses this capability as its ["Special Sauce"](https://jeremydmiller.com/2018/01/16/introducing-bluemilk-structuremaps-replacement-jaspers-special-sauce/) (Lamar was originally built
under the project name "BlueMilk"). See also [Roslyn Powered Code Weaving Middleware](https://jeremydmiller.com/2018/05/16/roslyn-powered-code-weaving-middleware/) for more information about how Jasper
uses Lamar for its efficient middleware strategy.
