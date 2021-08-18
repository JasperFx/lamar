import{o as n,c as a,a as s,d as t,e,b as p}from"./app.80913d4a.js";const o='{"title":"Try Getting an Optional Service by Plugin Type and Name","description":"","frontmatter":{},"relativePath":"guide/ioc/resolving/try-getting-an-optional-service-by-plugin-type-and-name.md","lastUpdated":1629293895205}',c={},i=t("h1",{id:"try-getting-an-optional-service-by-plugin-type-and-name"},[t("a",{class:"header-anchor",href:"#try-getting-an-optional-service-by-plugin-type-and-name","aria-hidden":"true"},"#"),e(" Try Getting an Optional Service by Plugin Type and Name")],-1),u=t("p",null,[e("Just use the "),t("code",null,"IContainer.TryGetInstance<T>(name)"),e(" or "),t("code",null,"IContainer.TryGetInstance(Type pluginType, string name)"),e(" method as shown below:")],-1),l=p('<p><a id="snippet-sample_trygetinstancevianameandgeneric_returnsinstance_whentypefound"></a></p><div class="language-cs"><pre><code><span class="token punctuation">[</span><span class="token attribute"><span class="token class-name">Fact</span></span><span class="token punctuation">]</span>\n<span class="token keyword">public</span> <span class="token return-type class-name"><span class="token keyword">void</span></span> <span class="token function">TryGetInstanceViaNameAndGeneric_ReturnsInstance_WhenTypeFound</span><span class="token punctuation">(</span><span class="token punctuation">)</span>\n<span class="token punctuation">{</span>\n    <span class="token function">addColorInstance</span><span class="token punctuation">(</span><span class="token string">&quot;Red&quot;</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n    <span class="token function">addColorInstance</span><span class="token punctuation">(</span><span class="token string">&quot;Orange&quot;</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n    <span class="token function">addColorInstance</span><span class="token punctuation">(</span><span class="token string">&quot;Blue&quot;</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n\n    <span class="token comment">// &quot;Orange&quot; exists, so an object should be returned</span>\n    <span class="token class-name"><span class="token keyword">var</span></span> instance <span class="token operator">=</span> _container<span class="token punctuation">.</span><span class="token generic-method"><span class="token function">TryGetInstance</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>Rule<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token string">&quot;Orange&quot;</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n    instance<span class="token punctuation">.</span><span class="token function">ShouldBeOfType</span><span class="token punctuation">(</span><span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">ColorRule</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n<span class="token punctuation">}</span>\n</code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Graph/ContainerTester.cs#L268-L281" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_trygetinstancevianameandgeneric_returnsinstance_whentypefound" title="Start of snippet">anchor</a></sup>\x3c!-- endSnippet --\x3e</p>',3);c.render=function(t,e,p,o,c,r){return n(),a("div",null,[i,u,s(" snippet: sample_TryGetInstanceViaNameAndGeneric_ReturnsInstance_WhenTypeFound "),l])};export{o as __pageData,c as default};
