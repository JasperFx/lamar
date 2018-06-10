<!--Title: Nested Containers (Per Request/Transaction)-->

<[info]>
If you're coming from StructureMap, do note that Lamar does not yet support the concept of "child" containers
<[/info]>

_Nested Container's_ are a powerful feature in Lamar for service resolution and clean object disposal in the 
context of short lived operations like HTTP requests or handling a message within some kind of service bus. A _nested container_
is specific to the scope of that operation and is should not live on outside of that scope.

Here's a sample of a nested container in action:

<[sample:using-nested-container]>

You probably won't directly interact with nested containers, but do note that they are used behind the scenes at runtime of basically every
popular application framework in .Net these days (ASP.Net Core, NServiceBus, MassTransit, NancyFx, you name it).

