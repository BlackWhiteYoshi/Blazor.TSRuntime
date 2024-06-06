using System.Reflection;

namespace TSRuntime.Tests;

public static class AssemblyNameAndVersionTest {
    [Fact]
    public static void AssemblyNameAndVersionMatch() {
        string name = (string)typeof(Generation.Builder).GetField("ASSEMBLY_NAME", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
        string version = (string)typeof(Generation.Builder).GetField("ASSEMBLY_VERSION", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;

        string assemblyName = typeof(TSRuntimeGenerator).Assembly.GetName().Name!;
        string assemblyVersion = typeof(TSRuntimeGenerator).Assembly.GetName().Version!.ToString()[..^2];
        
        Assert.Equal(name, assemblyName);
        Assert.Equal(version, assemblyVersion);
    }
}
