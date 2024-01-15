# Changing Configuration at Runtime

::: warning
Don't do this, you've been warned.
:::

The Lamar team respectfully recommends that you don't do this, but this functionality is here because we had to have this for
Jasper's integration with ASP.Net Core. Please note that this should only be used **additively**. Unlike StructureMap, Lamar will not rewrite build
plans for existing registrations to accommodate changes here.

<!-- snippet: sample_add_all_new_services -->
<a id='snippet-sample_add_all_new_services'></a>
```cs
[Fact]
public void add_all_new_services()
{
    var container = new Container(_ => { _.AddTransient<IWidget, RedWidget>(); });

    container.Configure(_ => _.AddTransient<IService, WhateverService>());

    container.GetInstance<IService>()
        .ShouldBeOfType<WhateverService>();
}
```
<sup><a href='https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/configure_container.cs#L12-L25' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_add_all_new_services' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
