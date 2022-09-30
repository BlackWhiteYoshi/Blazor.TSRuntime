using TSRuntime.Core.Configs;
using Xunit;

namespace TSRuntime.Core.Tests;

public class CoreConfigTest {
    // TODO after config is established

    [Fact]
    public void Config_FieldsAreNotEmpty() {
        Config config = new();

        Assert.NotNull(config.FileOutputClass);
        Assert.NotNull(config.FileOutputinterface);
        Assert.NotEmpty(config.TypeMap);
        Assert.NotEmpty(config.UsingStatements);
    }
}
