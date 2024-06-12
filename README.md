# TSRuntime

An improved JSRuntime with

- automatic JS-module loading and caching
- compile time errors instead of runtime errors
- IntelliSense guidance


<!-- JS example code -->
<div style="display: table; margin-inline: auto; width: 50%">
<p style="background-color: #404040; padding: 0.4em 1em; margin: 2em 0 0 0">Example.razor.js</p>

<pre style="overflow-x: hidden"><code><span style="color: #57A64A">/**</span>
 <span style="color: #57A64A">*</span> @<span style="color: #569CD6">param</span> {string} <span style="color: #9CDCFE">name</span>
 <span style="color: #57A64A">*</span> @<span style="color: #569CD6">param</span> {number} <span style="color: #9CDCFE">myNumber</span>
 <span style="color: #57A64A">*/</span>
<span style="color: #D8A0DF">export</span> <span style="color: #569CD6">function</span> <span style="color: #DCDCAA">saveNumber</span>(<span style="color: #9CDCFE">name</span>, <span style="color: #9CDCFE">myNumber</span>) {
    <span style="color: #9CDCFE">localStorage</span>.<span style="color: #DCDCAA">setItem</span>(<span style="color: #9CDCFE">name</span>, <span style="color: #9CDCFE">myNumber</span>.<span style="color: #DCDCAA">toString</span>());
}
</code></pre>
</div>


<div style="
    display: grid;
    grid-auto-flow: column;
    grid-template-rows: auto auto auto;
    grid-template-columns: 49% 49%;
    column-gap: 2%;
    margin-top: 3em;">


<!-- JSRuntime Header -->
<b style="display:table; margin-inline: auto;">JSRuntime</b>

<!-- JSRuntime example -->
<div>
<p style="background-color: #404040; padding: 0.4em 1em; margin: 2em 0 0 0">Example.razor.cs</p>

<pre style="overflow-x: hidden"><code>[<span style="color: #4EC9B0">Inject</span>]
<span style="color: #569CD6">public required</span> <span style="color: #B8D7A3">IJSRuntime</span> JsRuntime { <span style="color: #569CD6">private get</span>; <span style="color: #569CD6">init</span>; }

<span style="color: #569CD6">public async</span> <span style="color: #4EC9B0">Task</span> <span style="color: #DCDCAA">DoJsStuff</span>()
{
    <span style="color: #57A64A">// InvokeAsync<T>(string identifier, object?[]? args)</span>
    <span style="color: #569CD6">await using</span> <span style="color: #B8D7A3">IJSObjectReference</span> <span style="color: #9CDCFE">module</span> = <span style="color: #569CD6">await</span> JsRuntime.
        <span style="color: #DCDCAA">InvokeAsync</span>&lt;<span style="color: #B8D7A3">IJSObjectReference</span>&gt;(<span style="color: #D69D85">"import"</span>, <span style="color: #D69D85">"./Example.razor.js"</span>);

    <span style="color: #57A64A">// InvokeAsync<T>(string identifier, object?[]? args)</span>
    <span style="color: #569CD6">await</span> <span style="color: #9CDCFE">module</span>.<span style="color: #DCDCAA">InvokeVoidAsync</span>(<span style="color: #D69D85">"saveNumber"</span>, <span style="color: #D69D85">"save1"</span>, <span style="color: #B5CEA8">5</span>);
}
</code></pre>
</div>

<!-- JSRuntime bullet points -->
<div>

- not cached
- JS-function or params not match -> no warnings or errors
- bad IntelliSense support

</div>

<!-- TSRuntime Header -->

<b style="display:table; margin-inline: auto;">TSRuntime</b>

<!-- TSRuntime example -->
<div>
<p style="background-color: #404040; padding: 0.4em 1em; margin: 2em 0 0 0">Example.razor.cs</p>

<pre style="overflow-x: hidden"><code>[<span style="color: #4EC9B0">Inject</span>]
<span style="color: #569CD6">public required</span> <span style="color: #B8D7A3">ITSRuntime</span> TsRuntime { <span style="color: #569CD6">private get</span>; <span style="color: #569CD6">init</span>; }

<span style="color: #569CD6">public async</span> <span style="color: #4EC9B0">Task</span> <span style="color: #DCDCAA">DoJsStuff</span>()
{
    
    <span style="color: #57A64A">// module is imported and cached automatically</span>


    <span style="color: #57A64A">// SaveNumber&lt;int&gt;(string name, int myNumber)</span>
    <span style="color: #569CD6">await</span> TsRuntime.<span style="color: #DCDCAA">SaveNumber</span>(<span style="color: #D69D85">"save1"</span>, <span style="color: #B5CEA8">5</span>);
}
</code></pre>
</div>

<!-- TSRuntime bullet points -->
<div>

- imported and cached automatically
- JS-function or params not match -> compile error
- nice IntelliSense guidance

</div>


</div>
