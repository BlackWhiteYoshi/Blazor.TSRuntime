namespace TSRuntime.Core.Generation;

public static partial class Generator {
    public static string TSRuntimeContent => """
        // --- <auto generated> ---

        using System.Threading;
        using System.Threading.Tasks;

        namespace Microsoft.JSInterop;

        public sealed class TSRuntime : ITSRuntime, IDisposable, IAsyncDisposable
        {
            #region construction

            private readonly IJSRuntime _jsRuntime;
            IJSRuntime ITSRuntime.JsRuntime => _jsRuntime;


            public TSRuntime(IJSRuntime jsRuntime)
            {
                _jsRuntime = jsRuntime;
            }

            #endregion


            #region disposing

            private readonly CancellationTokenSource cancellationTokenSource = new();

            public void Dispose()
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();

                for (int i = 0; i < modules.Length; i++)
                {
                    Task<IJSObjectReference>? module = modules[i];

                    if (module?.IsCompletedSuccessfully == true)
                        _ = module.Result.DisposeAsync().Preserve();

                    modules[i] = null;
                }
            }

            public ValueTask DisposeAsync()
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();

                List<Task> taskList = new();
                for (int i = 0; i < modules.Length; i++)
                {
                    Task<IJSObjectReference>? module = modules[i];

                    if (module?.IsCompletedSuccessfully == true)
                    {
                        ValueTask valueTask = module.Result.DisposeAsync();
                        if (!valueTask.IsCompleted)
                            taskList.Add(valueTask.AsTask());
                    }

                    modules[i] = null;
                }

                if (taskList.Count == 0)
                    return ValueTask.CompletedTask;
                else
                    return new ValueTask(Task.WhenAll(taskList));
            }

            #endregion


            #region moduleList

            private readonly Task<IJSObjectReference>?[] modules = new Task<IJSObjectReference>?[ITSRuntime.MODULE_COUNT];

            Task<IJSObjectReference> ITSRuntime.GetOrLoadModule(int index, string url)
                => modules[index] ??= _jsRuntime.InvokeAsync<IJSObjectReference>("import", cancellationTokenSource.Token, url).AsTask();

            #endregion
        }
        
        """;
}
