# Lamar and IDisposable

One of the main reasons to use an IoC container is to offload the work of disposing created objects at the right time in the application scope. Sure, it's something you should be aware of, but developers are less likely to make mistakes if that's just handled for them.

## Singletons

This one is easy, any object marked as the _Singleton_ lifecycle that implements `IDisposable` is disposed when the root container is
disposed:

<[sample:singleton-in-action]>

## Nested Containers

As discussed in [nested containers](/guide/ioc/nested-containers), any transient or container-scoped object that implements `IDisposable` and is created
by a nested container will be disposed as the nested container is disposed:

<[sample:nested-disposal]>

## Transients

<[warning]>
This behavior is different from StructureMap. Be aware of this, or you may be vulnerable to memory leaks.
<[/warning]>

Objects that implement `IDisposable` are tracked by the container that creates them and will be disposed whenever that container itself is disposed.
