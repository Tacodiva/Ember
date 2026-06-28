
using System;
using System.IO;
using Json.Schema;

namespace Ember;

internal static class EmbeddedResources {

    private const string EmbeddedResourcePrefix = "Ember.Embedded.";

    private static StreamReader CreateStreamReader(string name) {
        return new(
            typeof(EmbeddedResources).Assembly.GetManifestResourceStream(EmbeddedResourcePrefix + name)
                ?? throw new FileNotFoundException($"Could not find expected embedded resource '{EmbeddedResourcePrefix}{name}'.")
        );
    }

    private static string ReadString(string name) {
        using StreamReader reader = CreateStreamReader(name);
        return reader.ReadToEnd();
    }

    private static JsonSchema LoadSchema(string name) {
        return JsonSchema.FromText(ReadString(name));
    }

    public static readonly JsonSchema LogConfigSchema = LoadSchema("LogConfigSchema.json");
}