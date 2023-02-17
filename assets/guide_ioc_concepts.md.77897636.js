import{_ as s,c as e,o as n,a}from"./app.8aee9f9c.js";const D=JSON.parse('{"title":"Software Design Concepts","description":"","frontmatter":{},"headers":[{"level":2,"title":"Inversion of Control","slug":"inversion-of-control","link":"#inversion-of-control","children":[]},{"level":2,"title":"Dependency Injection","slug":"dependency-injection","link":"#dependency-injection","children":[]},{"level":2,"title":"Service Locator","slug":"service-locator","link":"#service-locator","children":[]}],"relativePath":"guide/ioc/concepts.md"}'),o={name:"guide/ioc/concepts.md"},t=a(`<h1 id="software-design-concepts" tabindex="-1">Software Design Concepts <a class="header-anchor" href="#software-design-concepts" aria-hidden="true">#</a></h1><h2 id="inversion-of-control" tabindex="-1">Inversion of Control <a class="header-anchor" href="#inversion-of-control" aria-hidden="true">#</a></h2><p>Years ago I consulted for a company that had developed a successful software engine for pricing and analyzing potential energy trades. The next step for them was to adapt their pricing engine so that it could be embedded in other software packages or even a simple spreadsheet so that analysts could quickly try out &quot;what if&quot; scenarios before making any kind of deal. The immediate problem this firm had was that their pricing engine was architected such that the pricing engine business logic directly invoked their proprietary database schema and configuration files. The strategic pricing engine logic was effectively useless without all the rest of their system, so forget embedding the logic into spreadsheet logic.</p><p>With the benefit of hindsight, if we were to build an energy trading pricing engine from scratch, we would probably opt to use the software design concept of <em><a href="https://en.wikipedia.org/wiki/Inversion_of_control" target="_blank" rel="noreferrer">Inversion of Control</a></em> such that the actual pricing logic code would be handed all the pricing metadata it needed to perform its work instead of making the pricing logic reach out to get it. In its most general usage, <em>Inversion of Control</em> simply means that a component is given some sort of dependent data or service or configuration instead of that component having to &quot;know&quot; how to fetch or find that resource.</p><p>An IoC container like Lamar uses the <em>Inversion of Control</em> concept to simplify your internal services by freeing them from having to know how to find, build, or clean up their dependencies.</p><h2 id="dependency-injection" tabindex="-1">Dependency Injection <a class="header-anchor" href="#dependency-injection" aria-hidden="true">#</a></h2><p><em><a href="https://en.wikipedia.org/wiki/Dependency_injection" target="_blank" rel="noreferrer">Dependency Injection</a></em> is nothing more than pushing dependencies of an object into constructor functions or setter properties instead of that object doing everything for itself. If you are strictly using <em>Dependency Injection</em> to fill the dependencies of your classes, your code should have no coupling to Lamar itself.</p><p><a id="snippet-sample_basic-dependency-injection"></a></p><div class="language-cs"><button title="Copy Code" class="copy"></button><span class="lang">cs</span><pre class="shiki material-theme-palenight" tabindex="0"><code><span class="line"><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#F78C6C;">interface</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">IDatabase</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">{</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">}</span></span>
<span class="line"></span>
<span class="line"><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#F78C6C;">class</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">DatabaseUser</span></span>
<span class="line"><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#89DDFF;">    </span><span style="color:#676E95;font-style:italic;">// Using Constructor Injection</span></span>
<span class="line"><span style="color:#A6ACCD;">    </span><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#82AAFF;">DatabaseUser</span><span style="color:#89DDFF;">(</span><span style="color:#FFCB6B;">IDatabase</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">database</span><span style="color:#89DDFF;">)</span></span>
<span class="line"><span style="color:#A6ACCD;">    </span><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#A6ACCD;">    </span><span style="color:#89DDFF;">}</span></span>
<span class="line"><span style="color:#89DDFF;">}</span></span>
<span class="line"></span>
<span class="line"><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#F78C6C;">class</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">OtherDatabaseUser</span></span>
<span class="line"><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#89DDFF;">    </span><span style="color:#676E95;font-style:italic;">// Setter Injection</span></span>
<span class="line"><span style="color:#A6ACCD;">    </span><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">IDatabase</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">Database</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">{</span><span style="color:#A6ACCD;"> </span><span style="color:#F78C6C;">get</span><span style="color:#89DDFF;">;</span><span style="color:#A6ACCD;"> </span><span style="color:#F78C6C;">set</span><span style="color:#89DDFF;">;</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">}</span></span>
<span class="line"><span style="color:#89DDFF;">}</span></span>
<span class="line"></span></code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/DependencyInjection.cs#L5-L22" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_basic-dependency-injection" title="Start of snippet">anchor</a></sup></p><h2 id="service-locator" tabindex="-1">Service Locator <a class="header-anchor" href="#service-locator" aria-hidden="true">#</a></h2><p>Lamar also fills the role of a <em><a href="https://en.wikipedia.org/wiki/Service_locator_pattern" target="_blank" rel="noreferrer">service locator</a></em>. In this usage, your code would directly access Lamar&#39;s <code>Container</code> class to build or resolve services upon demand like this sample:</p><p><a id="snippet-sample_basic-service-location"></a></p><div class="language-cs"><button title="Copy Code" class="copy"></button><span class="lang">cs</span><pre class="shiki material-theme-palenight" tabindex="0"><code><span class="line"><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#F78C6C;">class</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">ThirdDatabaseUser</span></span>
<span class="line"><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#A6ACCD;">    </span><span style="color:#C792EA;">private</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">IDatabase</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">_database</span><span style="color:#89DDFF;">;</span></span>
<span class="line"></span>
<span class="line"><span style="color:#A6ACCD;">    </span><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#82AAFF;">ThirdDatabaseUser</span><span style="color:#89DDFF;">(</span><span style="color:#FFCB6B;">IContainer</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">container</span><span style="color:#89DDFF;">)</span></span>
<span class="line"><span style="color:#A6ACCD;">    </span><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#89DDFF;">        </span><span style="color:#676E95;font-style:italic;">// This is service location</span></span>
<span class="line"><span style="color:#A6ACCD;">        _database </span><span style="color:#89DDFF;">=</span><span style="color:#A6ACCD;"> container</span><span style="color:#89DDFF;">.</span><span style="color:#82AAFF;">GetInstance</span><span style="color:#89DDFF;">&lt;</span><span style="color:#FFCB6B;">IDatabase</span><span style="color:#89DDFF;">&gt;();</span></span>
<span class="line"><span style="color:#A6ACCD;">    </span><span style="color:#89DDFF;">}</span></span>
<span class="line"><span style="color:#89DDFF;">}</span></span>
<span class="line"></span></code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/DependencyInjection.cs#L24-L36" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_basic-service-location" title="Start of snippet">anchor</a></sup></p><p>Since IoC tools like Lamar have come onto the software scene, many developers have very badly overused the service locator pattern and many other developers have become very vocal in their distaste for service location. The Lamar team simply recommends that you favor Dependency Injection wherever possible, but that <em>some</em> service location in your system where you may need more advanced building options or lazy resolution of services is probably just fine.</p>`,16),l=[t];function p(r,c,i,d,y,h){return n(),e("div",null,l)}const F=s(o,[["render",p]]);export{D as __pageData,F as default};