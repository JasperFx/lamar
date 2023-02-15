import{_ as s,c as e,o as a,a as n}from"./app.5b13aa3a.js";const u=JSON.parse('{"title":"Environment Tests","description":"","frontmatter":{},"headers":[],"relativePath":"guide/ioc/diagnostics/environment-tests.md"}'),t={name:"guide/ioc/diagnostics/environment-tests.md"},o=n(`<h1 id="environment-tests" tabindex="-1">Environment Tests <a class="header-anchor" href="#environment-tests" aria-hidden="true">#</a></h1><p>Years ago I worked with a legacy system that was particularly fragile in its deployment. While my team at the time and I made some serious improvements in the reliability of the automated deployment, the best thing we did was to add a set of <em>environment tests</em> to the deployment that verified that basic elements of the system were working like:</p><ul><li>Could our code access the configured database?</li><li>Was a certain COM object registered on the server? (I hated COM then and the years haven&#39;t changed my mind)</li><li>Could we connect via remoting to another deployed application?</li></ul><p>The deployments still frequently failed, but we were able to spot <strong>and diagnose</strong> the underlying problems much faster with our new environment tests than we could before by trying to run and debug the not-quite-valid application.</p><p>One of the mechanisms we used for these environment tests was Lamar&#39;s ability to mark methods on configured types as environment tests with the <code>[ValidationMethod]</code> attribute as shown below:</p><p><a id="snippet-sample_validation-method-usage"></a></p><div class="language-cs"><button title="Copy Code" class="copy"></button><span class="lang">cs</span><pre class="shiki material-theme-palenight" tabindex="0"><code><span class="line"><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#F78C6C;">class</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">Database</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">:</span><span style="color:#A6ACCD;"> </span><span style="color:#FFCB6B;">IDatabase</span></span>
<span class="line"><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#A6ACCD;">    </span><span style="color:#89DDFF;">[</span><span style="color:#FFCB6B;">ValidationMethod</span><span style="color:#89DDFF;">]</span></span>
<span class="line"><span style="color:#A6ACCD;">    </span><span style="color:#C792EA;">public</span><span style="color:#A6ACCD;"> </span><span style="color:#89DDFF;">void</span><span style="color:#A6ACCD;"> </span><span style="color:#82AAFF;">TryToConnect</span><span style="color:#89DDFF;">()</span></span>
<span class="line"><span style="color:#A6ACCD;">    </span><span style="color:#89DDFF;">{</span></span>
<span class="line"><span style="color:#89DDFF;">        </span><span style="color:#676E95;font-style:italic;">// try to open a connection to the configured</span></span>
<span class="line"><span style="color:#89DDFF;">        </span><span style="color:#676E95;font-style:italic;">// database connection string</span></span>
<span class="line"></span>
<span class="line"><span style="color:#89DDFF;">        </span><span style="color:#676E95;font-style:italic;">// throw an exception if the database cannot</span></span>
<span class="line"><span style="color:#89DDFF;">        </span><span style="color:#676E95;font-style:italic;">// be reached</span></span>
<span class="line"><span style="color:#A6ACCD;">    </span><span style="color:#89DDFF;">}</span></span>
<span class="line"><span style="color:#89DDFF;">}</span></span>
<span class="line"></span></code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/StructureMap.Testing/Examples/ValidationMethod.cs#L12-L25" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_validation-method-usage" title="Start of snippet">anchor</a></sup></p><p>Used in conjunction with <a href="/lamar/guide/ioc/diagnostics/validating-container-configuration.html">Lamar&#39;s ability to validate a container</a>, you can use this technique to quickly support environment tests embedded into your system code.</p>`,9),l=[o];function i(p,r,c,d,h,y){return a(),e("div",null,l)}const D=s(t,[["render",i]]);export{u as __pageData,D as default};