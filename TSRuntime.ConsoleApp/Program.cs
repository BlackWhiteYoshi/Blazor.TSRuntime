using TSRuntime.Core.Parsing;
using TSRuntime.Core.Configs;
using TSRuntime.Core.Generation;
using System.Text;

namespace TSRuntime.ConsoleApp;

public sealed class Program {
    public static async Task Main() {
        string json = File.ReadAllText("tsconfig.tsruntime.json");
        Config config = Config.FromJson(json);

        TSSyntaxTree syntaxTree = new();
        syntaxTree.ParseModules(config.DeclarationPath);

        byte[] buffer = new byte[65536];

        using (FileStream fileStream = new(config.FileOutputClass, FileMode.Create, FileAccess.Write)) {
            int count = Encoding.UTF8.GetBytes(Generator.TSRuntimeContent, buffer);
            await fileStream.WriteAsync(buffer.AsMemory(0, count));
        }

        using (FileStream fileStream = new(config.FileOutputinterface, FileMode.Create, FileAccess.Write)) {
            foreach (string fragment in Generator.GetITSRuntimeContent(syntaxTree, config)) {
                int count = Encoding.UTF8.GetBytes(fragment, buffer);
                await fileStream.WriteAsync(buffer.AsMemory(0, count));
            }
        }
    }
}
