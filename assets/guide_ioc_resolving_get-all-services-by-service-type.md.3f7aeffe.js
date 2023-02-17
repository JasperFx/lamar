import{_ as n,c as s,o as a,a as t}from"./app.35d600bb.js";const m='{"title":"Get all Services by Service Type","description":"","frontmatter":{},"relativePath":"guide/ioc/resolving/get-all-services-by-service-type.md","lastUpdated":1644423598339}',p={},e=t(`<h1 id="get-all-services-by-service-type" tabindex="-1">Get all Services by Service Type <a class="header-anchor" href="#get-all-services-by-service-type" aria-hidden="true">#</a></h1><div class="warning custom-block"><p class="custom-block-title">WARNING</p><p>The functionality respects the order in which the actual instances are configured in the Container -- which is compliant with the expected behavior inside of <a href="http://ASP.Net" target="_blank" rel="noopener noreferrer">ASP.Net</a> Core. Be warned that some other IoC tools make different assumptions if you are coming from a different tool.</p></div><p>Please see <a href="/guide/ioc/working-with-enumerable-types.html">working with Enumerable Types</a> for a lot more information about what&#39;s going on behind the scenes.</p><p>Once in a while you might want to get an enumerable of all the configured objects for a ServiceType. That&#39;s done with the <code>GetAllInstances()</code> method shown below:</p><p><a id="snippet-sample_get-all-instances"></a></p><div class="language-cs"><pre><code><span class="token punctuation">[</span><span class="token attribute"><span class="token class-name">Fact</span></span><span class="token punctuation">]</span>
<span class="token keyword">public</span> <span class="token return-type class-name"><span class="token keyword">void</span></span> <span class="token function">get_all_instances</span><span class="token punctuation">(</span><span class="token punctuation">)</span>
<span class="token punctuation">{</span>
    <span class="token class-name"><span class="token keyword">var</span></span> container <span class="token operator">=</span> <span class="token keyword">new</span> <span class="token constructor-invocation class-name">Container</span><span class="token punctuation">(</span>x <span class="token operator">=&gt;</span>
    <span class="token punctuation">{</span>
        x<span class="token punctuation">.</span><span class="token generic-method"><span class="token function">For</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>IWidget<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">.</span><span class="token generic-method"><span class="token function">Add</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>AWidget<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">.</span><span class="token function">Named</span><span class="token punctuation">(</span><span class="token string">&quot;A&quot;</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
        x<span class="token punctuation">.</span><span class="token generic-method"><span class="token function">For</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>IWidget<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">.</span><span class="token generic-method"><span class="token function">Add</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>BWidget<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">.</span><span class="token function">Named</span><span class="token punctuation">(</span><span class="token string">&quot;B&quot;</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
        x<span class="token punctuation">.</span><span class="token generic-method"><span class="token function">For</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>IWidget<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">.</span><span class="token generic-method"><span class="token function">Add</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>CWidget<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">.</span><span class="token function">Named</span><span class="token punctuation">(</span><span class="token string">&quot;C&quot;</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
    <span class="token punctuation">}</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
    
    container<span class="token punctuation">.</span><span class="token generic-method"><span class="token function">QuickBuildAll</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>IWidget<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span>
        <span class="token punctuation">.</span><span class="token function">Select</span><span class="token punctuation">(</span>x <span class="token operator">=&gt;</span> x<span class="token punctuation">.</span><span class="token function">GetType</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">)</span>
        <span class="token punctuation">.</span><span class="token function">ShouldHaveTheSameElementsAs</span><span class="token punctuation">(</span><span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">AWidget</span><span class="token punctuation">)</span><span class="token punctuation">,</span> <span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">BWidget</span><span class="token punctuation">)</span><span class="token punctuation">,</span> <span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">CWidget</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

    container<span class="token punctuation">.</span><span class="token generic-method"><span class="token function">GetAllInstances</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>IWidget<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span>
        <span class="token punctuation">.</span><span class="token function">Select</span><span class="token punctuation">(</span>x <span class="token operator">=&gt;</span> x<span class="token punctuation">.</span><span class="token function">GetType</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">)</span>
        <span class="token punctuation">.</span><span class="token function">ShouldHaveTheSameElementsAs</span><span class="token punctuation">(</span><span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">AWidget</span><span class="token punctuation">)</span><span class="token punctuation">,</span> <span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">BWidget</span><span class="token punctuation">)</span><span class="token punctuation">,</span> <span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">CWidget</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

    <span class="token comment">// or</span>

    container<span class="token punctuation">.</span><span class="token function">GetAllInstances</span><span class="token punctuation">(</span><span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">IWidget</span><span class="token punctuation">)</span><span class="token punctuation">)</span>
        <span class="token punctuation">.</span><span class="token generic-method"><span class="token function">OfType</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>IWidget<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span> <span class="token comment">// returns an IEnumerable, so I&#39;m casting here</span>
        <span class="token punctuation">.</span><span class="token function">Select</span><span class="token punctuation">(</span>x <span class="token operator">=&gt;</span> x<span class="token punctuation">.</span><span class="token function">GetType</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">)</span>
        <span class="token punctuation">.</span><span class="token function">ShouldHaveTheSameElementsAs</span><span class="token punctuation">(</span><span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">AWidget</span><span class="token punctuation">)</span><span class="token punctuation">,</span> <span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">BWidget</span><span class="token punctuation">)</span><span class="token punctuation">,</span> <span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">CWidget</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
<span class="token punctuation">}</span>
</code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/IoC/Acceptance/get_all_instances.cs#L87-L114" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_get-all-instances" title="Start of snippet">anchor</a></sup><a id="snippet-sample_get-all-instances-1"></a></p><div class="language-cs"><pre><code><span class="token punctuation">[</span><span class="token attribute"><span class="token class-name">Fact</span></span><span class="token punctuation">]</span>
<span class="token keyword">public</span> <span class="token return-type class-name"><span class="token keyword">void</span></span> <span class="token function">get_all_instances</span><span class="token punctuation">(</span><span class="token punctuation">)</span>
<span class="token punctuation">{</span>
    <span class="token class-name"><span class="token keyword">var</span></span> container <span class="token operator">=</span> <span class="token keyword">new</span> <span class="token constructor-invocation class-name">Container</span><span class="token punctuation">(</span>x <span class="token operator">=&gt;</span>
    <span class="token punctuation">{</span>
        x<span class="token punctuation">.</span><span class="token generic-method"><span class="token function">For</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>IWidget<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">.</span><span class="token generic-method"><span class="token function">Add</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>AWidget<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">.</span><span class="token function">Named</span><span class="token punctuation">(</span><span class="token string">&quot;A&quot;</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
        x<span class="token punctuation">.</span><span class="token generic-method"><span class="token function">For</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>IWidget<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">.</span><span class="token generic-method"><span class="token function">Add</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>BWidget<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">.</span><span class="token function">Named</span><span class="token punctuation">(</span><span class="token string">&quot;B&quot;</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
        x<span class="token punctuation">.</span><span class="token generic-method"><span class="token function">For</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>IWidget<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">.</span><span class="token generic-method"><span class="token function">Add</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>CWidget<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">.</span><span class="token function">Named</span><span class="token punctuation">(</span><span class="token string">&quot;C&quot;</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
    <span class="token punctuation">}</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

    container<span class="token punctuation">.</span><span class="token generic-method"><span class="token function">GetAllInstances</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>IWidget<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span>
        <span class="token punctuation">.</span><span class="token function">Select</span><span class="token punctuation">(</span>x <span class="token operator">=&gt;</span> x<span class="token punctuation">.</span><span class="token function">GetType</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">)</span>
        <span class="token punctuation">.</span><span class="token function">ShouldHaveTheSameElementsAs</span><span class="token punctuation">(</span><span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">AWidget</span><span class="token punctuation">)</span><span class="token punctuation">,</span> <span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">BWidget</span><span class="token punctuation">)</span><span class="token punctuation">,</span> <span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">CWidget</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

    <span class="token comment">// or</span>

    container<span class="token punctuation">.</span><span class="token function">GetAllInstances</span><span class="token punctuation">(</span><span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">IWidget</span><span class="token punctuation">)</span><span class="token punctuation">)</span>
        <span class="token punctuation">.</span><span class="token generic-method"><span class="token function">OfType</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>IWidget<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span> <span class="token comment">// returns an IEnumerable, so I&#39;m casting here</span>
        <span class="token punctuation">.</span><span class="token function">Select</span><span class="token punctuation">(</span>x <span class="token operator">=&gt;</span> x<span class="token punctuation">.</span><span class="token function">GetType</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">)</span>
        <span class="token punctuation">.</span><span class="token function">ShouldHaveTheSameElementsAs</span><span class="token punctuation">(</span><span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">AWidget</span><span class="token punctuation">)</span><span class="token punctuation">,</span> <span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">BWidget</span><span class="token punctuation">)</span><span class="token punctuation">,</span> <span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">CWidget</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
<span class="token punctuation">}</span>
</code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/Resolving/SimpleScenarios.cs#L50-L73" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_get-all-instances-1" title="Start of snippet">anchor</a></sup></p>`,9),o=[e];function c(u,l,i,k,r,d){return a(),s("div",null,o)}var f=n(p,[["render",c]]);export{m as __pageData,f as default};