import{o as n,c as a,a as s,b as t}from"./app.213acb6a.js";const e='{"title":"Building Custom Frames","description":"","frontmatter":{},"headers":[{"level":2,"title":"Creating a Variable within a Frame","slug":"creating-a-variable-within-a-frame"},{"level":2,"title":"Finding Dependent Variables","slug":"finding-dependent-variables"}],"relativePath":"guide/compilation/frames/frame.md","lastUpdated":1629732345322}',p={},o=t('<h1 id="building-custom-frames"><a class="header-anchor" href="#building-custom-frames" aria-hidden="true">#</a> Building Custom Frames</h1><div class="tip custom-block"><p class="custom-block-title">INFO</p><p>If you&#39;re going to get into LamarCodeGeneration&#39;s model, you probably want to be familiar and comfortable with both string interpolation in C# and the recent <code>nameof</code> operator.</p></div><p>To build a custom frame, you first need to create a new class that subclasses <code>Frame</code>, with these other more specific subclasses to start from as well:</p><ul><li><code>SyncFrame</code> - a frame that generates purely synchronous code</li><li><code>AsyncFrame</code> - a frame that has at least one <code>await</code> call in the code generated</li></ul><p>The one thing you absolutely have to do when you create a new <code>Frame</code> class is to override the <code>GenerateCode()</code> method. Take this example from Lamar itself for a frame that just injects a comment into the generated code:</p>',5),c=t('<p><a id="snippet-sample_commentframe"></a></p><div class="language-cs"><pre><code><span class="token keyword">public</span> <span class="token keyword">class</span> <span class="token class-name">CommentFrame</span> <span class="token punctuation">:</span> <span class="token type-list"><span class="token class-name">SyncFrame</span></span>\n<span class="token punctuation">{</span>\n    <span class="token keyword">private</span> <span class="token keyword">readonly</span> <span class="token class-name"><span class="token keyword">string</span></span> _commentText<span class="token punctuation">;</span>\n\n    <span class="token keyword">public</span> <span class="token function">CommentFrame</span><span class="token punctuation">(</span><span class="token class-name"><span class="token keyword">string</span></span> commentText<span class="token punctuation">)</span>\n    <span class="token punctuation">{</span>\n        _commentText <span class="token operator">=</span> commentText<span class="token punctuation">;</span>\n    <span class="token punctuation">}</span>\n\n    <span class="token keyword">public</span> <span class="token keyword">override</span> <span class="token return-type class-name"><span class="token keyword">void</span></span> <span class="token function">GenerateCode</span><span class="token punctuation">(</span><span class="token class-name">GeneratedMethod</span> method<span class="token punctuation">,</span> <span class="token class-name">ISourceWriter</span> writer<span class="token punctuation">)</span>\n    <span class="token punctuation">{</span>\n        writer<span class="token punctuation">.</span><span class="token function">WriteComment</span><span class="token punctuation">(</span>_commentText<span class="token punctuation">)</span><span class="token punctuation">;</span>\n        \n        <span class="token comment">// It&#39;s on you to call through to a possible next</span>\n        <span class="token comment">// frame to let it generate its code</span>\n        Next<span class="token punctuation">?.</span><span class="token function">GenerateCode</span><span class="token punctuation">(</span>method<span class="token punctuation">,</span> writer<span class="token punctuation">)</span><span class="token punctuation">;</span>\n    <span class="token punctuation">}</span>\n<span class="token punctuation">}</span>\n</code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/LamarCodeGeneration/Frames/Frame.cs#L16-L35" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_commentframe" title="Start of snippet">anchor</a></sup>\x3c!-- endSnippet --\x3e</p><p>A couple things to note about the <code>GenerateCode()</code> method:</p><ul><li>The <code>GeneratedMethod</code> will tell you information about the new method being generated like the return type and whether or not the method returns a <code>Task</code> or is marked with the <code>async</code> keyword.</li><li>You use the <code>ISourceWriter</code> argument to write new code into the generated method</li><li>It&#39;s your responsibility to call the <code>Next?.GenerateCode()</code> method to give the next frame a chance to write its code. <strong>Don&#39;t forget to do this step</strong>.</li></ul><p>Inside a custom frame, you can also nest the code from the frames following yours in a method. See this frame from Lamar itself that calls a &quot;no arg&quot; constructor on a concrete class and returns a variable. In the case of a class that implements <code>IDisposable</code>, it should write a C# <code>using</code> block that surrounds the inner code:</p>',6),l=t('<p><a id="snippet-sample_noargcreationframe"></a></p><div class="language-cs"><pre><code><span class="token keyword">public</span> <span class="token keyword">class</span> <span class="token class-name">NoArgCreationFrame</span> <span class="token punctuation">:</span> <span class="token type-list"><span class="token class-name">SyncFrame</span></span>\n<span class="token punctuation">{</span>\n    <span class="token keyword">public</span> <span class="token function">NoArgCreationFrame</span><span class="token punctuation">(</span><span class="token class-name">Type</span> concreteType<span class="token punctuation">)</span> \n    <span class="token punctuation">{</span>\n        <span class="token comment">// By creating the variable this way, we&#39;re</span>\n        <span class="token comment">// marking the variable as having been created</span>\n        <span class="token comment">// by this frame</span>\n        Output <span class="token operator">=</span> <span class="token keyword">new</span> <span class="token constructor-invocation class-name">Variable</span><span class="token punctuation">(</span>concreteType<span class="token punctuation">,</span> <span class="token keyword">this</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n    <span class="token punctuation">}</span>\n\n    <span class="token keyword">public</span> <span class="token return-type class-name">Variable</span> Output <span class="token punctuation">{</span> <span class="token keyword">get</span><span class="token punctuation">;</span> <span class="token punctuation">}</span>\n\n    <span class="token comment">// You have to override this method</span>\n    <span class="token keyword">public</span> <span class="token keyword">override</span> <span class="token return-type class-name"><span class="token keyword">void</span></span> <span class="token function">GenerateCode</span><span class="token punctuation">(</span><span class="token class-name">GeneratedMethod</span> method<span class="token punctuation">,</span> <span class="token class-name">ISourceWriter</span> writer<span class="token punctuation">)</span>\n    <span class="token punctuation">{</span>\n        <span class="token class-name"><span class="token keyword">var</span></span> creation <span class="token operator">=</span> <span class="token interpolation-string"><span class="token string">$&quot;var </span><span class="token interpolation"><span class="token punctuation">{</span><span class="token expression language-csharp">Output<span class="token punctuation">.</span>Usage</span><span class="token punctuation">}</span></span><span class="token string"> = new </span><span class="token interpolation"><span class="token punctuation">{</span><span class="token expression language-csharp">Output<span class="token punctuation">.</span>VariableType<span class="token punctuation">.</span><span class="token function">FullNameInCode</span><span class="token punctuation">(</span><span class="token punctuation">)</span></span><span class="token punctuation">}</span></span><span class="token string">()&quot;</span></span><span class="token punctuation">;</span>\n\n        <span class="token keyword">if</span> <span class="token punctuation">(</span>Output<span class="token punctuation">.</span>VariableType<span class="token punctuation">.</span><span class="token generic-method"><span class="token function">CanBeCastTo</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>IDisposable<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">)</span>\n        <span class="token punctuation">{</span>\n            <span class="token comment">// there is an ISourceWriter shortcut for this, but this makes</span>\n            <span class="token comment">// a better code demo;)</span>\n            writer<span class="token punctuation">.</span><span class="token function">Write</span><span class="token punctuation">(</span><span class="token interpolation-string"><span class="token string">$&quot;BLOCK:using (</span><span class="token interpolation"><span class="token punctuation">{</span><span class="token expression language-csharp">creation</span><span class="token punctuation">}</span></span><span class="token string">)&quot;</span></span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n            Next<span class="token punctuation">?.</span><span class="token function">GenerateCode</span><span class="token punctuation">(</span>method<span class="token punctuation">,</span> writer<span class="token punctuation">)</span><span class="token punctuation">;</span>\n            writer<span class="token punctuation">.</span><span class="token function">FinishBlock</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n        <span class="token punctuation">}</span>\n        <span class="token keyword">else</span>\n        <span class="token punctuation">{</span>\n            writer<span class="token punctuation">.</span><span class="token function">WriteLine</span><span class="token punctuation">(</span>creation <span class="token operator">+</span> <span class="token string">&quot;;&quot;</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n            Next<span class="token punctuation">?.</span><span class="token function">GenerateCode</span><span class="token punctuation">(</span>method<span class="token punctuation">,</span> writer<span class="token punctuation">)</span><span class="token punctuation">;</span>\n        <span class="token punctuation">}</span>\n    <span class="token punctuation">}</span>\n<span class="token punctuation">}</span>\n</code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Examples/NoArgConstructor.cs#L9-L42" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_noargcreationframe" title="Start of snippet">anchor</a></sup>\x3c!-- endSnippet --\x3e</p><h2 id="creating-a-variable-within-a-frame"><a class="header-anchor" href="#creating-a-variable-within-a-frame" aria-hidden="true">#</a> Creating a Variable within a Frame</h2><p>If the code generated by a <code>Frame</code> creates a new <code>Variable</code> in the generated code, it should set itself as the creator of that variable. You can do that by either passing a frame into a variable as its creator like this line from the <code>NoArgCreationFrame</code> shown above:</p>',5),i=t('<p><a id="snippet-sample_noargcreationframector"></a></p><div class="language-cs"><pre><code><span class="token keyword">public</span> <span class="token function">NoArgCreationFrame</span><span class="token punctuation">(</span><span class="token class-name">Type</span> concreteType<span class="token punctuation">)</span> \n<span class="token punctuation">{</span>\n    <span class="token comment">// By creating the variable this way, we&#39;re</span>\n    <span class="token comment">// marking the variable as having been created</span>\n    <span class="token comment">// by this frame</span>\n    Output <span class="token operator">=</span> <span class="token keyword">new</span> <span class="token constructor-invocation class-name">Variable</span><span class="token punctuation">(</span>concreteType<span class="token punctuation">,</span> <span class="token keyword">this</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n<span class="token punctuation">}</span>\n</code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Examples/NoArgConstructor.cs#L47-L55" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_noargcreationframector" title="Start of snippet">anchor</a></sup>\x3c!-- endSnippet --\x3e</p><p>Otherwise, you could also have written that code like this:</p>',4),u=t('<p><a id="snippet-sample_noargcreationframector2"></a></p><div class="language-cs"><pre><code><span class="token keyword">public</span> <span class="token function">NoArgCreationFrame</span><span class="token punctuation">(</span><span class="token class-name">Type</span> concreteType<span class="token punctuation">)</span> \n<span class="token punctuation">{</span>\n    <span class="token comment">// By creating the variable this way, we&#39;re</span>\n    <span class="token comment">// marking the variable as having been created</span>\n    <span class="token comment">// by this frame</span>\n    Output <span class="token operator">=</span> <span class="token function">Create</span><span class="token punctuation">(</span>concreteType<span class="token punctuation">)</span><span class="token punctuation">;</span>\n<span class="token punctuation">}</span>\n</code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/Lamar.Testing/Examples/NoArgConstructor.cs#L58-L66" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_noargcreationframector2" title="Start of snippet">anchor</a></sup>\x3c!-- endSnippet --\x3e</p><h2 id="finding-dependent-variables"><a class="header-anchor" href="#finding-dependent-variables" aria-hidden="true">#</a> Finding Dependent Variables</h2><p>The other main thing you need to know is how to locate <code>Variable</code> objects your <code>Frame</code> needs to use. You accomplish that by overriding the <code>FindVariables()</code> method. Take this example below that is used within Lamar to generate code that resolves a service by calling a <a href="https://en.wikipedia.org/wiki/Service_locator_pattern" target="_blank" rel="noopener noreferrer">service locator</a> method on a Lamar <code>Scope</code> (a nested container most likely) object:</p>',5),r=t('<p><a id="snippet-sample_getinstanceframe"></a></p><div class="language-cs"><pre><code><span class="token keyword">public</span> <span class="token keyword">class</span> <span class="token class-name">GetInstanceFrame</span> <span class="token punctuation">:</span> <span class="token type-list"><span class="token class-name">SyncFrame</span><span class="token punctuation">,</span> <span class="token class-name">IResolverFrame</span></span>\n<span class="token punctuation">{</span>\n    <span class="token keyword">private</span> <span class="token keyword">static</span> <span class="token keyword">readonly</span> <span class="token class-name">MethodInfo</span> _resolveMethod <span class="token operator">=</span>\n        ReflectionHelper<span class="token punctuation">.</span><span class="token generic-method"><span class="token function">GetMethod</span><span class="token generic class-name"><span class="token punctuation">&lt;</span>Instance<span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span>x <span class="token operator">=&gt;</span> x<span class="token punctuation">.</span><span class="token function">Resolve</span><span class="token punctuation">(</span><span class="token keyword">null</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n    \n    \n    \n    <span class="token keyword">private</span> <span class="token class-name">Variable</span> _scope<span class="token punctuation">;</span>\n    <span class="token keyword">private</span> <span class="token keyword">readonly</span> <span class="token class-name"><span class="token keyword">string</span></span> _name<span class="token punctuation">;</span>\n\n    <span class="token keyword">public</span> <span class="token function">GetInstanceFrame</span><span class="token punctuation">(</span><span class="token class-name">Instance</span> instance<span class="token punctuation">)</span>\n    <span class="token punctuation">{</span>\n        Variable <span class="token operator">=</span> <span class="token keyword">new</span> <span class="token constructor-invocation class-name">ServiceVariable</span><span class="token punctuation">(</span>instance<span class="token punctuation">,</span> <span class="token keyword">this</span><span class="token punctuation">,</span> ServiceDeclaration<span class="token punctuation">.</span>ServiceType<span class="token punctuation">)</span><span class="token punctuation">;</span>\n        \n        _name <span class="token operator">=</span> instance<span class="token punctuation">.</span>Name<span class="token punctuation">;</span>\n    <span class="token punctuation">}</span>\n\n    <span class="token keyword">public</span> <span class="token keyword">override</span> <span class="token return-type class-name"><span class="token keyword">void</span></span> <span class="token function">GenerateCode</span><span class="token punctuation">(</span><span class="token class-name">GeneratedMethod</span> method<span class="token punctuation">,</span> <span class="token class-name">ISourceWriter</span> writer<span class="token punctuation">)</span>\n    <span class="token punctuation">{</span>\n        writer<span class="token punctuation">.</span><span class="token function">Write</span><span class="token punctuation">(</span><span class="token interpolation-string"><span class="token string">$&quot;var </span><span class="token interpolation"><span class="token punctuation">{</span><span class="token expression language-csharp">Variable<span class="token punctuation">.</span>Usage</span><span class="token punctuation">}</span></span><span class="token string"> = </span><span class="token interpolation"><span class="token punctuation">{</span><span class="token expression language-csharp">_scope<span class="token punctuation">.</span>Usage</span><span class="token punctuation">}</span></span><span class="token string">.</span><span class="token interpolation"><span class="token punctuation">{</span><span class="token expression language-csharp"><span class="token keyword">nameof</span><span class="token punctuation">(</span>Scope<span class="token punctuation">.</span>GetInstance<span class="token punctuation">)</span></span><span class="token punctuation">}</span></span><span class="token string">&lt;</span><span class="token interpolation"><span class="token punctuation">{</span><span class="token expression language-csharp">Variable<span class="token punctuation">.</span>VariableType<span class="token punctuation">.</span><span class="token function">FullNameInCode</span><span class="token punctuation">(</span><span class="token punctuation">)</span></span><span class="token punctuation">}</span></span><span class="token string">&gt;(\\&quot;</span><span class="token interpolation"><span class="token punctuation">{</span><span class="token expression language-csharp">_name</span><span class="token punctuation">}</span></span><span class="token string">\\&quot;);&quot;</span></span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n        Next<span class="token punctuation">?.</span><span class="token function">GenerateCode</span><span class="token punctuation">(</span>method<span class="token punctuation">,</span> writer<span class="token punctuation">)</span><span class="token punctuation">;</span>\n    <span class="token punctuation">}</span>\n\n    <span class="token keyword">public</span> <span class="token keyword">override</span> <span class="token return-type class-name">IEnumerable<span class="token punctuation">&lt;</span>Variable<span class="token punctuation">&gt;</span></span> <span class="token function">FindVariables</span><span class="token punctuation">(</span><span class="token class-name">IMethodVariables</span> chain<span class="token punctuation">)</span>\n    <span class="token punctuation">{</span>\n        _scope <span class="token operator">=</span> chain<span class="token punctuation">.</span><span class="token function">FindVariable</span><span class="token punctuation">(</span><span class="token keyword">typeof</span><span class="token punctuation">(</span><span class="token type-expression class-name">Scope</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n        <span class="token keyword">yield</span> <span class="token keyword">return</span> _scope<span class="token punctuation">;</span>\n    <span class="token punctuation">}</span>\n    \n    <span class="token keyword">public</span> <span class="token return-type class-name">ServiceVariable</span> Variable <span class="token punctuation">{</span> <span class="token keyword">get</span><span class="token punctuation">;</span> <span class="token punctuation">}</span>\n    \n    <span class="token keyword">public</span> <span class="token return-type class-name"><span class="token keyword">void</span></span> <span class="token function">WriteExpressions</span><span class="token punctuation">(</span><span class="token class-name">LambdaDefinition</span> definition<span class="token punctuation">)</span>\n    <span class="token punctuation">{</span>\n        <span class="token class-name"><span class="token keyword">var</span></span> scope <span class="token operator">=</span> definition<span class="token punctuation">.</span><span class="token function">Scope</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n        <span class="token class-name"><span class="token keyword">var</span></span> expr <span class="token operator">=</span> definition<span class="token punctuation">.</span><span class="token function">ExpressionFor</span><span class="token punctuation">(</span>Variable<span class="token punctuation">)</span><span class="token punctuation">;</span>\n\n        <span class="token class-name"><span class="token keyword">var</span></span> instance <span class="token operator">=</span> Variable<span class="token punctuation">.</span>Instance<span class="token punctuation">;</span>\n\n        <span class="token class-name"><span class="token keyword">var</span></span> @call <span class="token operator">=</span> Expression<span class="token punctuation">.</span><span class="token function">Call</span><span class="token punctuation">(</span>Expression<span class="token punctuation">.</span><span class="token function">Constant</span><span class="token punctuation">(</span>instance<span class="token punctuation">)</span><span class="token punctuation">,</span> _resolveMethod<span class="token punctuation">,</span> scope<span class="token punctuation">)</span><span class="token punctuation">;</span>\n        <span class="token class-name"><span class="token keyword">var</span></span> assign <span class="token operator">=</span> Expression<span class="token punctuation">.</span><span class="token function">Assign</span><span class="token punctuation">(</span>expr<span class="token punctuation">,</span> Expression<span class="token punctuation">.</span><span class="token function">Convert</span><span class="token punctuation">(</span>@call<span class="token punctuation">,</span> Variable<span class="token punctuation">.</span>VariableType<span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n        definition<span class="token punctuation">.</span>Body<span class="token punctuation">.</span><span class="token function">Add</span><span class="token punctuation">(</span>assign<span class="token punctuation">)</span><span class="token punctuation">;</span>\n\n        <span class="token keyword">if</span> <span class="token punctuation">(</span>Next <span class="token keyword">is</span> <span class="token class-name">IResolverFrame</span> next<span class="token punctuation">)</span>\n        <span class="token punctuation">{</span>\n            next<span class="token punctuation">.</span><span class="token function">WriteExpressions</span><span class="token punctuation">(</span>definition<span class="token punctuation">)</span><span class="token punctuation">;</span>\n        <span class="token punctuation">}</span>\n        <span class="token keyword">else</span>\n        <span class="token punctuation">{</span>\n            <span class="token keyword">throw</span> <span class="token keyword">new</span> <span class="token constructor-invocation class-name">InvalidCastException</span><span class="token punctuation">(</span><span class="token interpolation-string"><span class="token string">$&quot;</span><span class="token interpolation"><span class="token punctuation">{</span><span class="token expression language-csharp">Next<span class="token punctuation">.</span><span class="token function">GetType</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">.</span><span class="token function">GetFullName</span><span class="token punctuation">(</span><span class="token punctuation">)</span></span><span class="token punctuation">}</span></span><span class="token string"> does not implement </span><span class="token interpolation"><span class="token punctuation">{</span><span class="token expression language-csharp"><span class="token keyword">nameof</span><span class="token punctuation">(</span>IResolverFrame<span class="token punctuation">)</span></span><span class="token punctuation">}</span></span><span class="token string">&quot;</span></span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n        <span class="token punctuation">}</span>\n    <span class="token punctuation">}</span>\n<span class="token punctuation">}</span>\n</code></pre></div><p><sup><a href="https://github.com/JasperFx/lamar/blob/master/src/Lamar/IoC/Frames/GetInstanceFrame.cs#L15-L68" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_getinstanceframe" title="Start of snippet">anchor</a></sup>\x3c!-- endSnippet --\x3e</p><p>When you write a <code>FindVariables()</code> method, be sure to keep a reference to any variable you need for later, and return that variable as part of the enumeration from this method. Lamar uses the dependency relationship between frames, the variables they depend on, and the creators of those variables to correctly order and fill in any missing frames prior to generating code through the <code>GenerateCode()</code> method.</p>',4);p.render=function(t,e,p,k,d,m){return n(),a("div",null,[o,s(" snippet: sample_CommentFrame "),c,s(" snippet: sample_NoArgCreationFrame "),l,s(" snippet: sample_NoArgCreationFrameCtor "),i,s(" snippet: sample_NoArgCreationFrameCtor2 "),u,s(" snippet: sample_GetInstanceFrame "),r])};export{e as __pageData,p as default};