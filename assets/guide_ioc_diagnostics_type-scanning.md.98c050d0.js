import{_ as n,c as s,o as a,a as t}from"./app.35d600bb.js";const m='{"title":"Type Scanning Diagnostics","description":"","frontmatter":{},"headers":[{"level":2,"title":"Assert for Assembly Loading Failures","slug":"assert-for-assembly-loading-failures"},{"level":2,"title":"What did Lamar scan?","slug":"what-did-lamar-scan"}],"relativePath":"guide/ioc/diagnostics/type-scanning.md","lastUpdated":1644423598339}',e={},p=t(`<h1 id="type-scanning-diagnostics" tabindex="-1">Type Scanning Diagnostics <a class="header-anchor" href="#type-scanning-diagnostics" aria-hidden="true">#</a></h1><p>Type scanning and conventional auto-registration is a very powerful feature in Lamar, but it has been frequently troublesome to users when things go wrong. To try to alleviate problems, Lamar has some functionality for detecting and diagnosing problems with type scanning, mostly related to Assembly&#39;s being missing.</p><h2 id="assert-for-assembly-loading-failures" tabindex="-1">Assert for Assembly Loading Failures <a class="header-anchor" href="#assert-for-assembly-loading-failures" aria-hidden="true">#</a></h2><p>At its root, most type scanning and auto-registration schemes in .Net frameworks rely on the <a href="https://msdn.microsoft.com/en-us/library/system.reflection.assembly.getexportedtypes%28v=vs.110%29.aspx" target="_blank" rel="noopener noreferrer">Assembly.GetExportedTypes()</a> method. Unfortunately, that method can be brittle and fail whenever any dependency of that Assembly cannot be loaded into the current process, even if your application has no need for that dependency. In Lamar, you can use this method to assert the presence of any assembly load exceptions during type scanning:</p><p><a id="snippet-sample_assert-no-type-scanning-failures"></a></p><div class="language-cs"><pre><code>TypeRepository<span class="token punctuation">.</span><span class="token function">AssertNoTypeScanningFailures</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
</code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Graph/Scanning/TypeRepositoryTester.cs#L45-L47" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_assert-no-type-scanning-failures" title="Start of snippet">anchor</a></sup></p><p>The method above will throw an exception listing all the Assembly&#39;s that failed during the call to <code>GetExportedTypes()</code> only if there were any failures. Use this method during your application bootstrapping if you want it to fail fast with any type scanning problems.</p><h2 id="what-did-lamar-scan" tabindex="-1">What did Lamar scan? <a class="header-anchor" href="#what-did-lamar-scan" aria-hidden="true">#</a></h2><p>Confusion of type scanning has been a constant problem with Lamar usage over the years -- especially if users are trying to dynamically load assemblies from the file system for extensibility. In order to see into what Lamar has done with type scanning, 4.0 introduces the <code>Container.WhatDidIScan()</code> method.</p><p>Let&#39;s say that you have a <code>Container</code> that is set up with at least two different scanning operations like this sample from the Lamar unit tests:</p><p><a id="snippet-sample_whatdidiscan"></a></p><div class="language-cs"><pre><code><span class="token class-name"><span class="token keyword">var</span></span> container <span class="token operator">=</span> <span class="token keyword">new</span> <span class="token constructor-invocation class-name">Container</span><span class="token punctuation">(</span>_ <span class="token operator">=&gt;</span>
<span class="token punctuation">{</span>
    _<span class="token punctuation">.</span><span class="token function">Scan</span><span class="token punctuation">(</span>x <span class="token operator">=&gt;</span>
    <span class="token punctuation">{</span>
        x<span class="token punctuation">.</span><span class="token function">TheCallingAssembly</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

        x<span class="token punctuation">.</span><span class="token function">WithDefaultConventions</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
        x<span class="token punctuation">.</span><span class="token function">RegisterConcreteTypesAgainstTheFirstInterface</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
        x<span class="token punctuation">.</span><span class="token function">SingleImplementationsOfInterface</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
    <span class="token punctuation">}</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

    _<span class="token punctuation">.</span><span class="token function">Scan</span><span class="token punctuation">(</span>x <span class="token operator">=&gt;</span>
    <span class="token punctuation">{</span>
        <span class="token comment">// Give your scanning operation a descriptive name</span>
        <span class="token comment">// to help the diagnostics to be more useful</span>
        x<span class="token punctuation">.</span>Description <span class="token operator">=</span> <span class="token string">&quot;Second Scanner&quot;</span><span class="token punctuation">;</span>

        x<span class="token punctuation">.</span><span class="token function">AssembliesFromApplicationBaseDirectory</span><span class="token punctuation">(</span>assem <span class="token operator">=&gt;</span> assem<span class="token punctuation">.</span>FullName<span class="token punctuation">.</span><span class="token function">Contains</span><span class="token punctuation">(</span><span class="token string">&quot;Widget&quot;</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
        x<span class="token punctuation">.</span><span class="token function">ConnectImplementationsToTypesClosing</span><span class="token punctuation">(</span><span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">IService<span class="token punctuation">&lt;</span><span class="token punctuation">&gt;</span></span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
        x<span class="token punctuation">.</span><span class="token generic-method"><span class="token function">AddAllTypesOf</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>IWidget<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
    <span class="token punctuation">}</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
<span class="token punctuation">}</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

Console<span class="token punctuation">.</span><span class="token function">WriteLine</span><span class="token punctuation">(</span>container<span class="token punctuation">.</span><span class="token function">WhatDidIScan</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
</code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Diagnostics/WhatDidIScan_smoke_tests.cs#L21-L46" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_whatdidiscan" title="Start of snippet">anchor</a></sup><a id="snippet-sample_whatdidiscan-1"></a></p><div class="language-cs"><pre><code><span class="token class-name"><span class="token keyword">var</span></span> container <span class="token operator">=</span> <span class="token keyword">new</span> <span class="token constructor-invocation class-name">Container</span><span class="token punctuation">(</span>_ <span class="token operator">=&gt;</span>
<span class="token punctuation">{</span>
    _<span class="token punctuation">.</span><span class="token function">Scan</span><span class="token punctuation">(</span>x <span class="token operator">=&gt;</span>
    <span class="token punctuation">{</span>
        x<span class="token punctuation">.</span><span class="token function">TheCallingAssembly</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

        x<span class="token punctuation">.</span><span class="token function">WithDefaultConventions</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
        x<span class="token punctuation">.</span><span class="token function">RegisterConcreteTypesAgainstTheFirstInterface</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
        x<span class="token punctuation">.</span><span class="token function">SingleImplementationsOfInterface</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
    <span class="token punctuation">}</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

    _<span class="token punctuation">.</span><span class="token function">Scan</span><span class="token punctuation">(</span>x <span class="token operator">=&gt;</span>
    <span class="token punctuation">{</span>
        <span class="token comment">// Give your scanning operation a descriptive name</span>
        <span class="token comment">// to help the diagnostics to be more useful</span>
        x<span class="token punctuation">.</span>Description <span class="token operator">=</span> <span class="token string">&quot;Second Scanner&quot;</span><span class="token punctuation">;</span>

        x<span class="token punctuation">.</span><span class="token function">AssembliesFromApplicationBaseDirectory</span><span class="token punctuation">(</span>assem <span class="token operator">=&gt;</span> assem<span class="token punctuation">.</span>FullName<span class="token punctuation">.</span><span class="token function">Contains</span><span class="token punctuation">(</span><span class="token string">&quot;Widget&quot;</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
        x<span class="token punctuation">.</span><span class="token function">ConnectImplementationsToTypesClosing</span><span class="token punctuation">(</span><span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">IService<span class="token punctuation">&lt;</span><span class="token punctuation">&gt;</span></span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
        x<span class="token punctuation">.</span><span class="token generic-method"><span class="token function">AddAllTypesOf</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>IWidget<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
    <span class="token punctuation">}</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
<span class="token punctuation">}</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

Debug<span class="token punctuation">.</span><span class="token function">WriteLine</span><span class="token punctuation">(</span>container<span class="token punctuation">.</span><span class="token function">WhatDidIScan</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
</code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/WhatDidIScan_smoke_tester.cs#L14-L39" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_whatdidiscan-1" title="Start of snippet">anchor</a></sup></p><p>The resulting textual report is shown below:</p><p><em>Sorry for the formatting and color of the text, but the markdown engine does <strong>not</strong> like the textual report</em><a id="snippet-sample_whatdidiscan-result"></a></p><div class="language-cs"><pre><code><span class="token comment">/*
All Scanners
================================================================
Scanner #1
Assemblies
----------
* StructureMap.Testing, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null

Conventions
--------
* Default I[Name]/[Name] registration convention
* Register all concrete types against the first interface (if any) that they implement
* Register any single implementation of any interface against that interface

Second Scanner
Assemblies
----------
* StructureMap.Testing.GenericWidgets, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget2, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget3, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget4, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget5, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null

Conventions
--------
* Connect all implementations of open generic type IService&lt;T&gt;
* Find and register all types implementing StructureMap.Testing.Widget.IWidget

*/</span>
</code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Diagnostics/WhatDidIScan_smoke_tests.cs#L90-L121" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_whatdidiscan-result" title="Start of snippet">anchor</a></sup><a id="snippet-sample_whatdidiscan-result-1"></a></p><div class="language-cs"><pre><code><span class="token comment">/*
All Scanners
================================================================
Scanner #1
Assemblies
----------
* StructureMap.Testing, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null

Conventions
--------
* Default I[Name]/[Name] registration convention
* Register all concrete types against the first interface (if any) that they implement
* Register any single implementation of any interface against that interface

Second Scanner
Assemblies
----------
* StructureMap.Testing.GenericWidgets, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget2, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget3, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget4, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null
* StructureMap.Testing.Widget5, Version=4.0.0.51243, Culture=neutral, PublicKeyToken=null

Conventions
--------
* Connect all implementations of open generic type IService&lt;T&gt;
* Find and register all types implementing StructureMap.Testing.Widget.IWidget

*/</span>
</code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/WhatDidIScan_smoke_tester.cs#L43-L74" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_whatdidiscan-result-1" title="Start of snippet">anchor</a></sup></p><p>The textual report will show:</p><ol><li>All the scanning operations (calls to <code>Registry.Scan()</code>) with a descriptive name, either one supplied by you or the <code>Registry</code> type and an order number.</li><li>All the assemblies that were part of the scanning operation including the assembly name, version, and a warning if <code>Assembly.GetExportedTypes()</code> failed on that assembly.</li><li>All the configured scanning conventions inside of the scanning operation</li></ol><p><code>WhatDidIScan()</code> does not at this time show any type filters or exclusions that may be part of the assembly scanner.</p><p>See also: <a href="/guide/ioc/registration/auto-registration-and-conventions.html">Auto-Registration and Conventions</a></p>`,26),o=[p];function i(c,l,u,r,k,d){return a(),s("div",null,o)}var h=n(e,[["render",i]]);export{m as __pageData,h as default};
