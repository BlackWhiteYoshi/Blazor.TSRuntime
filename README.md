# TSRuntime

An improved JSRuntime with

- automatic JS-module loading and caching
- compile time errors instead of runtime errors
- IntelliSense guidance

<div style="display: flex; justify-content: space-between; margin-top: 2em;">

<div style="display: inline-block; width: 49%">

**JSRuntime**

<p style="background-color: #404040; padding: 0.4em 1em; margin: 2em 0 0 0">Example.razor.js</p>

<pre style="overflow-x: hidden"><code><span style="color: #D8A0DF">export</span> <span style="color: #569CD6">function</span> <span style="color: #DCDCAA">saveNumber</span>(<span style="color: #9CDCFE">name</span>, <span style="color: #9CDCFE">myNumber</span>) {
    <span style="color: #9CDCFE">localStorage</span>.<span style="color: #DCDCAA">setItem</span>(<span style="color: #9CDCFE">name</span>, <span style="color: #9CDCFE">myNumber</span>.<span style="color: #DCDCAA">toString</span>());
}
</code></pre>

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

- not cached
- JS-function or params not match -> no warnings or errors
- bad IntelliSense support

</div>


<div style="display: inline-block; width: 49%">

**TSRuntime**

<p style="background-color: #404040; padding: 0.4em 1em; margin: 2em 0 0 0">Example.razor.ts</p>

<pre style="overflow-x: hidden"><code><span style="color: #D8A0DF">export</span> <span style="color: #569CD6">function</span> <span style="color: #DCDCAA">saveNumber</span>(<span style="color: #9CDCFE">name</span>: <span style="color: #4EC9B0">string</span>, <span style="color: #9CDCFE">myNumber</span>: <span style="color: #4EC9B0">number</span>) {
    <span style="color: #9CDCFE">localStorage</span>.<span style="color: #DCDCAA">setItem</span>(<span style="color: #9CDCFE">name</span>, <span style="color: #9CDCFE">myNumber</span>.<span style="color: #DCDCAA">toString</span>());
}
</code></pre>

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

- imported and cached automatically
- JS-function or params not match -> compile error
- nice IntelliSense guidance

</div>

</div>
