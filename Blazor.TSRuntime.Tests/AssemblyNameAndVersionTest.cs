using TSRuntime.Generation;

namespace TSRuntime.Tests;

public sealed class AssemblyNameAndVersionTest {
    [Test]
    public async ValueTask AssemblyNameAndVersionMatch() {
        await Assert.That(typeof(TSRuntimeGenerator).Assembly.GetName().Name).IsEqualTo(AssemblyInfo.NAME);
        await Assert.That(typeof(TSRuntimeGenerator).Assembly.GetName().Version!.ToString(3)).IsEqualTo(AssemblyInfo.VERSION);
    }
}
