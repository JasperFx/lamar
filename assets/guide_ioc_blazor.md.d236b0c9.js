import{_ as s,c as a,o as n,a as l}from"./app.8aee9f9c.js";const A=JSON.parse('{"title":"Integration with Blazor","description":"","frontmatter":{},"headers":[],"relativePath":"guide/ioc/blazor.md"}'),o={name:"guide/ioc/blazor.md"},p=l(`<h1 id="integration-with-blazor" tabindex="-1">Integration with Blazor <a class="header-anchor" href="#integration-with-blazor" aria-hidden="true">#</a></h1><div class="warning custom-block"><p class="custom-block-title">WARNING</p><p>No specific Blazor support has been built into Lamar, and Blazor is still an immature and rapidly-evolving framework. Use of Lamar within Blazor apps is considered experimental.</p></div><p>To use Lamar within Blazor applications, you need only the base <a href="https://www.nuget.org/packages/Lamar/" target="_blank" rel="noreferrer">Lamar</a> NuGet package installed. Then, you can configure Blazor&#39;s host builder to use Lamar for IoC as shown below.</p><div class="language-csharp"><button title="Copy Code" class="copy"></button><span class="lang">csharp</span><pre class="shiki material-theme-palenight" tabindex="0"><code><span class="line"><span style="color:#F78C6C;">using</span><span style="color:#A6ACCD;"> System.Threading.Tasks</span><span style="color:#89DDFF;">;</span></span>
<span class="line"><span style="color:#F78C6C;">using</span><span style="color:#A6ACCD;"> Lamar</span><span style="color:#89DDFF;">;</span></span>
<span class="line"><span style="color:#F78C6C;">using</span><span style="color:#A6ACCD;"> Microsoft.AspNetCore.Components.WebAssembly.Hosting</span><span style="color:#89DDFF;">;</span></span>
<span class="line"></span>
<span class="line"><span style="color:#F78C6C;">namespace</span><span style="color:#A6ACCD;"> Lamar</span><span style="color:#89DDFF;">.</span><span style="color:#A6ACCD;">Sample</span><span style="color:#89DDFF;">.</span><span style="color:#A6ACCD;">Blazor</span></span>
<span class="line"><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#A6ACCD;">    </span><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#F78C6C;">class</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">Program</span></span>
<span class="line"><span style="color:#A6ACCD;">    </span><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#A6ACCD;">        </span><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#C792EA;">static</span><span style="color:#A6ACCD;"> </span><span style="color:#C792EA;">async</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">Task</span><span style="color:#A6ACCD;"> </span><span style="color:#82AAFF;">Main</span><span style="color:#89DDFF;">(string[]</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">args</span><span style="color:#89DDFF;">)</span></span>
<span class="line"><span style="color:#A6ACCD;">        </span><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#A6ACCD;">            </span><span style="color:#F78C6C;">var</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">builder</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">=</span><span style="color:#A6ACCD;"> WebAssemblyHostBuilder</span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">CreateDefault</span><span style="color:#89DDFF;">(</span><span style="color:#A6ACCD;">args</span><span style="color:#89DDFF;">);</span></span>
<span class="line"><span style="color:#A6ACCD;">            builder</span><span style="color:#89DDFF;">.</span><span style="color:#A6ACCD;">RootComponents</span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">Add</span><span style="color:#89DDFF;">&lt;</span><span style="color:#FFCB6B;">App</span><span style="color:#89DDFF;">&gt;(</span><span style="color:#89DDFF;">&quot;</span><span style="color:#C3E88D;">#app</span><span style="color:#89DDFF;">&quot;</span><span style="color:#89DDFF;">);</span></span>
<span class="line"></span>
<span class="line"><span style="color:#89DDFF;">            </span><span style="color:#676E95;font-style:italic;">// configure Blazor to use Lamar</span></span>
<span class="line"><span style="color:#A6ACCD;">            builder</span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">ConfigureContainer</span><span style="color:#89DDFF;">&lt;</span><span style="color:#FFCB6B;">ServiceRegistry</span><span style="color:#89DDFF;">&gt;(</span><span style="color:#F78C6C;">new</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">LamarServiceProviderFactory</span><span style="color:#89DDFF;">(),</span><span style="color:#A6ACCD;"> ConfigureServices</span><span style="color:#89DDFF;">);</span></span>
<span class="line"></span>
<span class="line"><span style="color:#A6ACCD;">            </span><span style="color:#F78C6C;">await</span><span style="color:#A6ACCD;"> builder</span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">Build</span><span style="color:#89DDFF;">().</span><span style="color:#82AAFF;">RunAsync</span><span style="color:#89DDFF;">();</span></span>
<span class="line"><span style="color:#A6ACCD;">        </span><span style="color:#89DDFF;">}</span></span>
<span class="line"></span>
<span class="line"><span style="color:#A6ACCD;">        </span><span style="color:#C792EA;">private</span><span style="color:#A6ACCD;"> </span><span style="color:#C792EA;">static</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">void</span><span style="color:#A6ACCD;"> </span><span style="color:#82AAFF;">ConfigureServices</span><span style="color:#89DDFF;">(</span><span style="color:#FFCB6B;">ServiceRegistry</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">services</span><span style="color:#89DDFF;">)</span></span>
<span class="line"><span style="color:#A6ACCD;">        </span><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#89DDFF;">            </span><span style="color:#676E95;font-style:italic;">// here you can configure Lamar as normal</span></span>
<span class="line"></span>
<span class="line"><span style="color:#A6ACCD;">            services</span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">For</span><span style="color:#89DDFF;">&lt;</span><span style="color:#FFCB6B;">IFoo</span><span style="color:#89DDFF;">&gt;().</span><span style="color:#82AAFF;">Use</span><span style="color:#89DDFF;">&lt;</span><span style="color:#FFCB6B;">Foo</span><span style="color:#89DDFF;">&gt;().</span><span style="color:#82AAFF;">Singleton</span><span style="color:#89DDFF;">();</span></span>
<span class="line"><span style="color:#A6ACCD;">            services</span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">IncludeRegistry</span><span style="color:#89DDFF;">&lt;</span><span style="color:#FFCB6B;">FooRegistry</span><span style="color:#89DDFF;">&gt;();</span></span>
<span class="line"><span style="color:#A6ACCD;">        </span><span style="color:#89DDFF;">}</span></span>
<span class="line"><span style="color:#A6ACCD;">    </span><span style="color:#89DDFF;">}</span></span>
<span class="line"><span style="color:#89DDFF;">}</span></span>
<span class="line"></span></code></pre></div><p>That&#39;s all you need; Blazor will now use Lamar for all dependency injection.</p>`,5),e=[p];function t(r,c,F,y,i,D){return n(),a("div",null,e)}const d=s(o,[["render",t]]);export{A as __pageData,d as default};