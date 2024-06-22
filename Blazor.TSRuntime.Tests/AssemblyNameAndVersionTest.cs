using TSRuntime.Generation;

namespace TSRuntime.Tests;

public static class AssemblyNameAndVersionTest {
    [Fact]
    public static void AssemblyNameAndVersionMatch() {
        Assert.Equal(AssemblyInfo.NAME, typeof(TSRuntimeGenerator).Assembly.GetName().Name);
        Assert.Equal(AssemblyInfo.VERSION, typeof(TSRuntimeGenerator).Assembly.GetName().Version!.ToString()[..^2]);
    }
}
