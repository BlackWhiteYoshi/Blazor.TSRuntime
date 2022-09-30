using System.Text;
using TSRuntime.Core.Configs;
using TSRuntime.Core.Generation;
using TSRuntime.Core.Parsing;
using Xunit;

namespace TSRuntime.Core.Tests;

public class CoreGeneratorTest {
    // No specific tests here to better adapt to changes.

    [Fact]
    public void TSRuntimeContent_IsNotEmpty() {
        Assert.NotNull(Generator.TSRuntimeContent);
        Assert.NotEqual(string.Empty, Generator.TSRuntimeContent);
    }

    [Fact]
    public void GetITSRuntimeContent_IsNotEmpty() {
        StringBuilder builder = new(10000);
        foreach (string str in Generator.GetITSRuntimeContent(new TSSyntaxTree(), new Config()))
            builder.Append(str);
        string result = builder.ToString();

        Assert.NotNull(result);
        Assert.NotEqual(string.Empty, Generator.TSRuntimeContent);
    }
}
