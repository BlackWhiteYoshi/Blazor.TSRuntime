using System.Text.Json.Nodes;

namespace TSRuntime.Core.Configs;

internal static class JsonNodeExtension {
    internal static string[] ToStringArray(this JsonNode node) => node.AsArray().Select((JsonNode? node) => (string?)node ?? throw NullNotAllowed).ToArray();

    internal static Dictionary<string, string> ToStringDictionary(this JsonNode node) {
        JsonObject jsonObject = node.AsObject();
        Dictionary<string, string> result = new(jsonObject.Count);

        foreach (KeyValuePair<string, JsonNode?> item in jsonObject)
            result.Add(item.Key, (string?)item.Value ?? throw NullNotAllowed);

        return result;
    }

    private static ArgumentException NullNotAllowed => new("null is not allowed - use string literal \"null\" instead");
}
