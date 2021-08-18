import{o as n,c as s,a,b as t}from"./app.80913d4a.js";const e='{"title":"Compiling Code with AssemblyGenerator","description":"","frontmatter":{},"relativePath":"guide/compilation/assembly-generator.md","lastUpdated":1629293895121}',p={},o=t('<h1 id="compiling-code-with-assemblygenerator"><a class="header-anchor" href="#compiling-code-with-assemblygenerator" aria-hidden="true">#</a> Compiling Code with AssemblyGenerator</h1><div class="tip custom-block"><p class="custom-block-title">INFO</p><p>The Lamar team thinks most users will use the <a href="/guide/compilation/frames/">Frames</a> to generate and compile code, but you might very well wish to bypass that admittedly complicated model and just use the inner utility classes that are shown in this page.</p></div><p>If all you want to do is take some C# code and compile that in memory to a new, in memory assembly, you can use the <code>LamarCompiler.AssemblyGenerator</code> class in the LamarCompiler library.</p><p>Let&#39;s say that you have a simple interface in your system like this:</p>',4),c=t('<p><a id="snippet-sample_ioperation"></a></p><div class="language-cs"><pre><code><span class="token keyword">public</span> <span class="token keyword">interface</span> <span class="token class-name">IOperation</span>\n<span class="token punctuation">{</span>\n    <span class="token return-type class-name"><span class="token keyword">int</span></span> <span class="token function">Calculate</span><span class="token punctuation">(</span><span class="token class-name"><span class="token keyword">int</span></span> one<span class="token punctuation">,</span> <span class="token class-name"><span class="token keyword">int</span></span> two<span class="token punctuation">)</span><span class="token punctuation">;</span>\n<span class="token punctuation">}</span>\n</code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/LamarCompiler.Testing/Samples/Codegen.cs#L9-L14" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_ioperation" title="Start of snippet">anchor</a></sup>\x3c!-- endSnippet --\x3e</p><p>Next, let&#39;s use <code>AssemblyGenerator</code> to compile code with a custom implementation of <code>IOperation</code> that we&#39;ve generated in code:</p>',4),l=t('<p><a id="snippet-sample_using-assemblygenerator"></a></p><div class="language-cs"><pre><code><span class="token class-name"><span class="token keyword">var</span></span> generator <span class="token operator">=</span> <span class="token keyword">new</span> <span class="token constructor-invocation class-name">AssemblyGenerator</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n\n<span class="token comment">// This is necessary for the compilation to succeed</span>\n<span class="token comment">// It&#39;s exactly the equivalent of adding references</span>\n<span class="token comment">// to your project</span>\ngenerator<span class="token punctuation">.</span><span class="token function">ReferenceAssembly</span><span class="token punctuation">(</span><span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">Console</span><span class="token punctuation">)</span><span class="token punctuation">.</span>Assembly<span class="token punctuation">)</span><span class="token punctuation">;</span>\ngenerator<span class="token punctuation">.</span><span class="token function">ReferenceAssembly</span><span class="token punctuation">(</span><span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">IOperation</span><span class="token punctuation">)</span><span class="token punctuation">.</span>Assembly<span class="token punctuation">)</span><span class="token punctuation">;</span>\n\n<span class="token comment">// Compile and generate a new .Net Assembly object</span>\n<span class="token comment">// in memory</span>\n<span class="token class-name"><span class="token keyword">var</span></span> assembly <span class="token operator">=</span> generator<span class="token punctuation">.</span><span class="token function">Generate</span><span class="token punctuation">(</span><span class="token string">@&quot;\nusing LamarCompiler.Testing.Samples;\n\nnamespace Generated\n{\npublic class AddOperator : IOperation\n{\npublic int Calculate(int one, int two)\n{\nreturn one + two;\n}\n}\n}\n&quot;</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n\n<span class="token comment">// Find the new type we generated up above</span>\n<span class="token class-name"><span class="token keyword">var</span></span> type <span class="token operator">=</span> assembly<span class="token punctuation">.</span><span class="token function">GetExportedTypes</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">.</span><span class="token function">Single</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n\n<span class="token comment">// Use Activator.CreateInstance() to build an object</span>\n<span class="token comment">// instance of our new class, and cast it to the </span>\n<span class="token comment">// IOperation interface</span>\n<span class="token class-name"><span class="token keyword">var</span></span> operation <span class="token operator">=</span> <span class="token punctuation">(</span>IOperation<span class="token punctuation">)</span>Activator<span class="token punctuation">.</span><span class="token function">CreateInstance</span><span class="token punctuation">(</span>type<span class="token punctuation">)</span><span class="token punctuation">;</span>\n\n<span class="token comment">// Use our new type</span>\n<span class="token class-name"><span class="token keyword">var</span></span> result <span class="token operator">=</span> operation<span class="token punctuation">.</span><span class="token function">Calculate</span><span class="token punctuation">(</span><span class="token number">1</span><span class="token punctuation">,</span> <span class="token number">2</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n</code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/LamarCompiler.Testing/Samples/Codegen.cs#L29-L66" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_using-assemblygenerator" title="Start of snippet">anchor</a></sup>\x3c!-- endSnippet --\x3e</p><p>There&#39;s only a couple things going on in the code above:</p><ol><li>I added an assembly reference for the .Net assembly that holds the <code>IOperation</code> interface</li><li>I passed a string to the <code>GenerateCode()</code> method, which successfully compiles my code and hands me back a .Net <a href="https://msdn.microsoft.com/en-us/library/system.reflection.assembly(v=vs.110).aspx" target="_blank" rel="noopener noreferrer">Assembly</a> object</li><li>Load the newly generated type from the new Assembly</li><li>Use the new <code>IOperation</code></li></ol><p>If you&#39;re not perfectly keen on doing brute force string manipulation to generate your code, you can also use Lamar&#39;s built in <a href="/guide/compilation/source-writer.html">ISourceWriter</a> to generate some of the code for you with all its code generation utilities:</p>',6),i=t('<p><a id="snippet-sample_using-assemblygenerator-with-source-writer"></a></p><div class="language-cs"><pre><code><span class="token class-name"><span class="token keyword">var</span></span> generator <span class="token operator">=</span> <span class="token keyword">new</span> <span class="token constructor-invocation class-name">AssemblyGenerator</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n\n<span class="token comment">// This is necessary for the compilation to succeed</span>\n<span class="token comment">// It&#39;s exactly the equivalent of adding references</span>\n<span class="token comment">// to your project</span>\ngenerator<span class="token punctuation">.</span><span class="token function">ReferenceAssembly</span><span class="token punctuation">(</span><span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">Console</span><span class="token punctuation">)</span><span class="token punctuation">.</span>Assembly<span class="token punctuation">)</span><span class="token punctuation">;</span>\ngenerator<span class="token punctuation">.</span><span class="token function">ReferenceAssembly</span><span class="token punctuation">(</span><span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">IOperation</span><span class="token punctuation">)</span><span class="token punctuation">.</span>Assembly<span class="token punctuation">)</span><span class="token punctuation">;</span>\n\n<span class="token class-name"><span class="token keyword">var</span></span> assembly <span class="token operator">=</span> generator<span class="token punctuation">.</span><span class="token function">Generate</span><span class="token punctuation">(</span>x <span class="token operator">=&gt;</span>\n<span class="token punctuation">{</span>\n    x<span class="token punctuation">.</span><span class="token function">Namespace</span><span class="token punctuation">(</span><span class="token string">&quot;Generated&quot;</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n    x<span class="token punctuation">.</span><span class="token function">StartClass</span><span class="token punctuation">(</span><span class="token string">&quot;AddOperator&quot;</span><span class="token punctuation">,</span> <span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">IOperation</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n    \n    x<span class="token punctuation">.</span><span class="token function">Write</span><span class="token punctuation">(</span><span class="token string">&quot;BLOCK:public int Calculate(int one, int two)&quot;</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n    x<span class="token punctuation">.</span><span class="token function">Write</span><span class="token punctuation">(</span><span class="token string">&quot;return one + two;&quot;</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n    x<span class="token punctuation">.</span><span class="token function">FinishBlock</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>  <span class="token comment">// Finish the method</span>\n    \n    x<span class="token punctuation">.</span><span class="token function">FinishBlock</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>  <span class="token comment">// Finish the class</span>\n    x<span class="token punctuation">.</span><span class="token function">FinishBlock</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>  <span class="token comment">// Finish the namespace</span>\n<span class="token punctuation">}</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n\n\n<span class="token class-name"><span class="token keyword">var</span></span> type <span class="token operator">=</span> assembly<span class="token punctuation">.</span><span class="token function">GetExportedTypes</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">.</span><span class="token function">Single</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n<span class="token class-name"><span class="token keyword">var</span></span> operation <span class="token operator">=</span> <span class="token punctuation">(</span>IOperation<span class="token punctuation">)</span>Activator<span class="token punctuation">.</span><span class="token function">CreateInstance</span><span class="token punctuation">(</span>type<span class="token punctuation">)</span><span class="token punctuation">;</span>\n\n<span class="token class-name"><span class="token keyword">var</span></span> result <span class="token operator">=</span> operation<span class="token punctuation">.</span><span class="token function">Calculate</span><span class="token punctuation">(</span><span class="token number">1</span><span class="token punctuation">,</span> <span class="token number">2</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n</code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/LamarCompiler.Testing/Samples/Codegen.cs#L73-L103" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_using-assemblygenerator-with-source-writer" title="Start of snippet">anchor</a></sup>\x3c!-- endSnippet --\x3e</p>',3);p.render=function(t,e,p,u,r,k){return n(),s("div",null,[o,a(" snippet: sample_IOperation "),c,a(" snippet: sample_using-AssemblyGenerator "),l,a(" snippet: sample_using-AssemblyGenerator-with-source-writer "),i])};export{e as __pageData,p as default};
