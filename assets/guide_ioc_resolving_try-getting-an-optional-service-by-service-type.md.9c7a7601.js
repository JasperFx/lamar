import{_ as s,c as n,o as a,a as p}from"./app.5b13aa3a.js";const A=JSON.parse('{"title":"Try Getting an Optional Service by Service Type","description":"","frontmatter":{},"headers":[{"level":2,"title":"Concrete Types","slug":"concrete-types","link":"#concrete-types","children":[]},{"level":2,"title":"Optional Generic Types","slug":"optional-generic-types","link":"#optional-generic-types","children":[]}],"relativePath":"guide/ioc/resolving/try-getting-an-optional-service-by-service-type.md"}'),l={name:"guide/ioc/resolving/try-getting-an-optional-service-by-service-type.md"},e=p(`<h1 id="try-getting-an-optional-service-by-service-type" tabindex="-1">Try Getting an Optional Service by Service Type <a class="header-anchor" href="#try-getting-an-optional-service-by-service-type" aria-hidden="true">#</a></h1><div class="tip custom-block"><p class="custom-block-title">INFO</p><p>The Lamar team does not recommend using &quot;optional&quot; dependencies as shown in this topic, but external frameworks like <a href="http://ASP.Net" target="_blank" rel="noreferrer">ASP.Net</a> MVC and Web API use this concept in their IoC container integration, so here it is. The Lamar team prefers the usage of the <a href="http://en.wikipedia.org/wiki/Null_Object_pattern" target="_blank" rel="noreferrer">Nullo pattern</a> instead.</p></div><p>In normal usage, if you ask Lamar for a service and Lamar doesn&#39;t recognize the requested type, the requested name, or know what the default should be for that type, Lamar will fail fast by throwing an exception rather than returning a null. Sometimes though, you may want to retrieve an <em>optional</em> service from Lamar that may or may not be registered in the Container. If that particular registration doesn&#39;t exist, you just want a null value. Lamar provides first class support for <em>optional</em> dependencies through the usage of the <code>IContainer.TryGetInstance()</code> methods.</p><div class="tip custom-block"><p class="custom-block-title">INFO</p><p>In Lamar, the <a href="http://ASP.Net" target="_blank" rel="noreferrer">ASP.Net</a> Core <code>IServiceProvider.GetService()</code> method has the same functionality and meaning as the <code>TryGetInstance()</code> method. If you were wondering how Lamar&#39;s StructureMap-flavored <code>GetInstance()</code> method is different, that&#39;s how.</p></div><p>Say you have a simple interface <code>IFoo</code> that may or may not be registered in the Container:</p><p><a id="snippet-sample_optional-foo"></a></p><div class="language-cs"><button title="Copy Code" class="copy"></button><span class="lang">cs</span><pre class="shiki material-theme-palenight" tabindex="0"><code><span class="line"><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#F78C6C;">interface</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">IFoo</span></span>
<span class="line"><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#89DDFF;">}</span></span>
<span class="line"></span>
<span class="line"><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#F78C6C;">class</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">Foo</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">:</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">IFoo</span></span>
<span class="line"><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#89DDFF;">}</span></span>
<span class="line"></span></code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/Resolving/OptionalDependencies.cs#L8-L17" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_optional-foo" title="Start of snippet">anchor</a></sup></p><p>In your own code you might request the <code>IFoo</code> service like the code below, knowing that you&#39;ll take responsibility yourself for building the <code>IFoo</code> service if Lamar doesn&#39;t have a registration for <code>IFoo</code>:</p><p><a id="snippet-sample_optional-real-usage"></a></p><div class="language-cs"><button title="Copy Code" class="copy"></button><span class="lang">cs</span><pre class="shiki material-theme-palenight" tabindex="0"><code><span class="line"><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#F78C6C;">class</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">MyFoo</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">:</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">IFoo</span></span>
<span class="line"><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#89DDFF;">}</span></span>
<span class="line"></span>
<span class="line"><span style="color:#89DDFF;">[</span><span style="color:#FFCB6B;">Fact</span><span style="color:#89DDFF;">]</span></span>
<span class="line"><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">void</span><span style="color:#A6ACCD;"> </span><span style="color:#82AAFF;">real_usage</span><span style="color:#89DDFF;">()</span></span>
<span class="line"><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#A6ACCD;">    </span><span style="color:#F78C6C;">var</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">container</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">=</span><span style="color:#A6ACCD;"> </span><span style="color:#F78C6C;">new</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">Container</span><span style="color:#89DDFF;">();</span></span>
<span class="line"></span>
<span class="line"><span style="color:#89DDFF;">    </span><span style="color:#676E95;font-style:italic;">// if the container doesn&#39;t know about it,</span></span>
<span class="line"><span style="color:#89DDFF;">    </span><span style="color:#676E95;font-style:italic;">// I&#39;ll build it myself</span></span>
<span class="line"><span style="color:#A6ACCD;">    </span><span style="color:#F78C6C;">var</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">foo</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">=</span><span style="color:#A6ACCD;"> container</span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">TryGetInstance</span><span style="color:#89DDFF;">&lt;</span><span style="color:#FFCB6B;">IFoo</span><span style="color:#89DDFF;">&gt;()</span></span>
<span class="line"><span style="color:#A6ACCD;">              </span><span style="color:#89DDFF;">??</span><span style="color:#A6ACCD;"> </span><span style="color:#F78C6C;">new</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">MyFoo</span><span style="color:#89DDFF;">();</span></span>
<span class="line"></span>
<span class="line"><span style="color:#89DDFF;">}</span></span>
<span class="line"></span></code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/Resolving/OptionalDependencies.cs#L95-L112" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_optional-real-usage" title="Start of snippet">anchor</a></sup></p><p>Just to make this perfectly clear, if Lamar has a default registration for <code>IFoo</code>, you get this behavior:</p><p><a id="snippet-sample_optional-got-it"></a></p><div class="language-cs"><button title="Copy Code" class="copy"></button><span class="lang">cs</span><pre class="shiki material-theme-palenight" tabindex="0"><code><span class="line"><span style="color:#89DDFF;">[</span><span style="color:#FFCB6B;">Fact</span><span style="color:#89DDFF;">]</span></span>
<span class="line"><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">void</span><span style="color:#A6ACCD;"> </span><span style="color:#82AAFF;">i_have_got_that</span><span style="color:#89DDFF;">()</span></span>
<span class="line"><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#A6ACCD;">    </span><span style="color:#F78C6C;">var</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">container</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">=</span><span style="color:#A6ACCD;"> </span><span style="color:#F78C6C;">new</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">Container</span><span style="color:#89DDFF;">(</span><span style="color:#FFCB6B;">_</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">=&gt;</span><span style="color:#A6ACCD;"> _</span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">For</span><span style="color:#89DDFF;">&lt;</span><span style="color:#FFCB6B;">IFoo</span><span style="color:#89DDFF;">&gt;().</span><span style="color:#82AAFF;">Use</span><span style="color:#89DDFF;">&lt;</span><span style="color:#FFCB6B;">Foo</span><span style="color:#89DDFF;">&gt;());</span></span>
<span class="line"></span>
<span class="line"><span style="color:#A6ACCD;">    container</span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">TryGetInstance</span><span style="color:#89DDFF;">&lt;</span><span style="color:#FFCB6B;">IFoo</span><span style="color:#89DDFF;">&gt;()</span></span>
<span class="line"><span style="color:#A6ACCD;">        </span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">ShouldNotBeNull</span><span style="color:#89DDFF;">();</span></span>
<span class="line"></span>
<span class="line"><span style="color:#89DDFF;">    </span><span style="color:#676E95;font-style:italic;">// -- or --</span></span>
<span class="line"></span>
<span class="line"><span style="color:#A6ACCD;">    container</span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">TryGetInstance</span><span style="color:#89DDFF;">(</span><span style="color:#F78C6C;">typeof</span><span style="color:#89DDFF;">(</span><span style="color:#FFCB6B;">IFoo</span><span style="color:#89DDFF;">))</span></span>
<span class="line"><span style="color:#A6ACCD;">        </span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">ShouldNotBeNull</span><span style="color:#89DDFF;">();</span></span>
<span class="line"><span style="color:#89DDFF;">}</span></span>
<span class="line"></span></code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/Resolving/OptionalDependencies.cs#L19-L34" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_optional-got-it" title="Start of snippet">anchor</a></sup></p><p>If Lamar knows nothing about <code>IFoo</code>, you get a null:</p><p><a id="snippet-sample_optional-dont-got-it"></a></p><div class="language-cs"><button title="Copy Code" class="copy"></button><span class="lang">cs</span><pre class="shiki material-theme-palenight" tabindex="0"><code><span class="line"><span style="color:#89DDFF;">[</span><span style="color:#FFCB6B;">Fact</span><span style="color:#89DDFF;">]</span></span>
<span class="line"><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">void</span><span style="color:#A6ACCD;"> </span><span style="color:#82AAFF;">i_do_not_have_that</span><span style="color:#89DDFF;">()</span></span>
<span class="line"><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#A6ACCD;">    </span><span style="color:#F78C6C;">var</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">container</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">=</span><span style="color:#A6ACCD;"> </span><span style="color:#F78C6C;">new</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">Container</span><span style="color:#89DDFF;">();</span></span>
<span class="line"></span>
<span class="line"><span style="color:#A6ACCD;">    container</span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">TryGetInstance</span><span style="color:#89DDFF;">&lt;</span><span style="color:#FFCB6B;">IFoo</span><span style="color:#89DDFF;">&gt;()</span></span>
<span class="line"><span style="color:#A6ACCD;">        </span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">ShouldBeNull</span><span style="color:#89DDFF;">();</span></span>
<span class="line"></span>
<span class="line"><span style="color:#89DDFF;">    </span><span style="color:#676E95;font-style:italic;">// -- or --</span></span>
<span class="line"></span>
<span class="line"><span style="color:#A6ACCD;">    container</span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">TryGetInstance</span><span style="color:#89DDFF;">(</span><span style="color:#F78C6C;">typeof</span><span style="color:#89DDFF;">(</span><span style="color:#FFCB6B;">IFoo</span><span style="color:#89DDFF;">))</span></span>
<span class="line"><span style="color:#A6ACCD;">        </span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">ShouldBeNull</span><span style="color:#89DDFF;">();</span></span>
<span class="line"><span style="color:#89DDFF;">}</span></span>
<span class="line"></span></code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/Resolving/OptionalDependencies.cs#L36-L51" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_optional-dont-got-it" title="Start of snippet">anchor</a></sup></p><h2 id="concrete-types" tabindex="-1">Concrete Types <a class="header-anchor" href="#concrete-types" aria-hidden="true">#</a></h2><p>Since it&#39;s not a perfect world, there are some gotchas you need to be aware of. While Lamar will happily <em>auto-resolve</em> concrete types that aren&#39;t registered, that does not apply to the <code>TryGetInstance</code> mechanism:</p><p><a id="snippet-sample_optional-no-concrete"></a></p><div class="language-cs"><button title="Copy Code" class="copy"></button><span class="lang">cs</span><pre class="shiki material-theme-palenight" tabindex="0"><code><span class="line"><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#F78C6C;">class</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">ConcreteThing</span></span>
<span class="line"><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#89DDFF;">}</span></span>
<span class="line"></span>
<span class="line"><span style="color:#89DDFF;">[</span><span style="color:#FFCB6B;">Fact</span><span style="color:#89DDFF;">]</span></span>
<span class="line"><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">void</span><span style="color:#A6ACCD;"> </span><span style="color:#82AAFF;">no_auto_resolution_of_concrete_types</span><span style="color:#89DDFF;">()</span></span>
<span class="line"><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#A6ACCD;">    </span><span style="color:#F78C6C;">var</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">container</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">=</span><span style="color:#A6ACCD;"> </span><span style="color:#F78C6C;">new</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">Container</span><span style="color:#89DDFF;">();</span></span>
<span class="line"></span>
<span class="line"><span style="color:#A6ACCD;">    container</span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">TryGetInstance</span><span style="color:#89DDFF;">&lt;</span><span style="color:#FFCB6B;">ConcreteThing</span><span style="color:#89DDFF;">&gt;()</span></span>
<span class="line"><span style="color:#A6ACCD;">        </span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">ShouldBeNull</span><span style="color:#89DDFF;">();</span></span>
<span class="line"></span>
<span class="line"><span style="color:#89DDFF;">    </span><span style="color:#676E95;font-style:italic;">// now register ConcreteThing and do it again</span></span>
<span class="line"><span style="color:#A6ACCD;">    container</span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">Configure</span><span style="color:#89DDFF;">(</span><span style="color:#FFCB6B;">_</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">=&gt;</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">{</span><span style="color:#A6ACCD;"> _</span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">For</span><span style="color:#89DDFF;">&lt;</span><span style="color:#FFCB6B;">ConcreteThing</span><span style="color:#89DDFF;">&gt;().</span><span style="color:#82AAFF;">Use</span><span style="color:#89DDFF;">&lt;</span><span style="color:#FFCB6B;">ConcreteThing</span><span style="color:#89DDFF;">&gt;();</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">});</span></span>
<span class="line"></span>
<span class="line"><span style="color:#A6ACCD;">    container</span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">TryGetInstance</span><span style="color:#89DDFF;">&lt;</span><span style="color:#FFCB6B;">ConcreteThing</span><span style="color:#89DDFF;">&gt;()</span></span>
<span class="line"><span style="color:#A6ACCD;">        </span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">ShouldNotBeNull</span><span style="color:#89DDFF;">();</span></span>
<span class="line"><span style="color:#89DDFF;">}</span></span>
<span class="line"></span></code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/Resolving/OptionalDependencies.cs#L53-L73" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_optional-no-concrete" title="Start of snippet">anchor</a></sup></p><h2 id="optional-generic-types" tabindex="-1">Optional Generic Types <a class="header-anchor" href="#optional-generic-types" aria-hidden="true">#</a></h2><p>If you are using open generic types, the <code>TryGetInstance()</code> mechanism <strong>can</strong> close the open generic registration to satisfy the optional dependency like this sample:</p><p><a id="snippet-sample_optional-close-generics"></a></p><div class="language-cs"><button title="Copy Code" class="copy"></button><span class="lang">cs</span><pre class="shiki material-theme-palenight" tabindex="0"><code><span class="line"><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#F78C6C;">interface</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">IThing</span><span style="color:#89DDFF;">&lt;</span><span style="color:#FFCB6B;">T</span><span style="color:#89DDFF;">&gt;</span></span>
<span class="line"><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#89DDFF;">}</span></span>
<span class="line"></span>
<span class="line"><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#F78C6C;">class</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">Thing</span><span style="color:#89DDFF;">&lt;</span><span style="color:#FFCB6B;">T</span><span style="color:#89DDFF;">&gt;</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">:</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">IThing</span><span style="color:#89DDFF;">&lt;</span><span style="color:#FFCB6B;">T</span><span style="color:#89DDFF;">&gt;</span></span>
<span class="line"><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#89DDFF;">}</span></span>
<span class="line"></span>
<span class="line"><span style="color:#89DDFF;">[</span><span style="color:#FFCB6B;">Fact</span><span style="color:#89DDFF;">]</span></span>
<span class="line"><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">void</span><span style="color:#A6ACCD;"> </span><span style="color:#82AAFF;">can_try_get_open_type_resolution</span><span style="color:#89DDFF;">()</span></span>
<span class="line"><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#A6ACCD;">    </span><span style="color:#F78C6C;">var</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">container</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">=</span><span style="color:#A6ACCD;"> </span><span style="color:#F78C6C;">new</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">Container</span><span style="color:#89DDFF;">(</span><span style="color:#FFCB6B;">_</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">=&gt;</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">{</span><span style="color:#A6ACCD;"> _</span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">For</span><span style="color:#89DDFF;">(</span><span style="color:#F78C6C;">typeof</span><span style="color:#89DDFF;">(</span><span style="color:#FFCB6B;">IThing</span><span style="color:#89DDFF;">&lt;&gt;)).</span><span style="color:#82AAFF;">Use</span><span style="color:#89DDFF;">(</span><span style="color:#F78C6C;">typeof</span><span style="color:#89DDFF;">(</span><span style="color:#FFCB6B;">Thing</span><span style="color:#89DDFF;">&lt;&gt;));</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">});</span></span>
<span class="line"></span>
<span class="line"><span style="color:#A6ACCD;">    container</span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">TryGetInstance</span><span style="color:#89DDFF;">&lt;</span><span style="color:#FFCB6B;">IThing</span><span style="color:#89DDFF;">&lt;string&gt;&gt;()</span></span>
<span class="line"><span style="color:#A6ACCD;">        </span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">ShouldBeOfType</span><span style="color:#89DDFF;">&lt;</span><span style="color:#FFCB6B;">Thing</span><span style="color:#89DDFF;">&lt;string&gt;&gt;();</span></span>
<span class="line"><span style="color:#89DDFF;">}</span></span>
<span class="line"></span></code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/Resolving/OptionalDependencies.cs#L75-L93" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_optional-close-generics" title="Start of snippet">anchor</a></sup></p>`,30),o=[e];function t(c,r,i,F,y,D){return a(),n("div",null,o)}const h=s(l,[["render",t]]);export{A as __pageData,h as default};