
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ember.Utils;
using Json.More;

namespace Ember.Logging;

public record LogConfigLevels(
    LogLevel DefaultLevel,
    Dictionary<string, LogLevel> LoggerLevels
) {
    public static readonly LogLevel DefaultDefaultLevel = LogLevel.Diagnostic;
    public static readonly LogConfigLevels Default = new(DefaultDefaultLevel, new());
}

public record LogHandlerConfig(
    string Name,
    bool Enabled,
    LogConfigLevels Levels,
    JsonObject? Settings
);

public interface ILogConfig {

    public LogConfigLevels GetGlobalLevels();
    public LogHandlerConfig GetHandlerConfig(string name);

}

public sealed class DefaultLogConfig : ILogConfig {
    public static readonly DefaultLogConfig Instance = new();

    private DefaultLogConfig() { }

    public LogConfigLevels GetGlobalLevels() => LogConfigLevels.Default;
    public LogHandlerConfig GetHandlerConfig(string name) => new(name, true, LogConfigLevels.Default, default);
}

public sealed class JsonLogConfig : ILogConfig {
    public static JsonLogConfig FromFile(string path) {
        return new(EmbeddedResources.LogConfigSchema.ValidateFile(path));
    }

    public static JsonLogConfig FromString(string str) {
        return new(EmbeddedResources.LogConfigSchema.ValidateString(str));
    }

    public static JsonLogConfig FromDocument(JsonDocument document) {
        return new(EmbeddedResources.LogConfigSchema.ValidateDocument(document));
    }

    private static IReadOnlyDictionary<string, LogLevel> CreateLevelsMap(JsonElement element) {
        Dictionary<string, LogLevel> map = new();
        foreach (JsonProperty prop in element.EnumerateObject())
            map[prop.Name] = (LogLevel)prop.Value.GetInt32();
        return map;
    }

    private readonly LogConfigLevels _globalLevels;
    private readonly Dictionary<string, LogHandlerConfig> _handlerConfigs;

    private JsonLogConfig(JsonDocument document) {

        LogConfigLevels GetLevels(JsonElement element) {
            LogLevel ParseLevel(JsonElement element) {
                return (LogLevel)element.GetUInt32();
            }

            LogLevel defaultLevel;

            if (element.TryGetProperty("Default", out JsonElement defaultElement)) {
                defaultLevel = ParseLevel(defaultElement);
            } else {
                defaultLevel = _globalLevels.DefaultLevel;
            }

            Dictionary<string, LogLevel> loggerLevels = new();

            if (element.TryGetProperty("Loggers", out JsonElement loggersElement)) {
                foreach (JsonProperty loggerProp in loggersElement.EnumerateObject()) {
                    loggerLevels[loggerProp.Name] = ParseLevel(loggerProp.Value);
                }
            }

            return new(defaultLevel, loggerLevels);
        }

        _globalLevels = LogConfigLevels.Default;
        if (document.RootElement.TryGetProperty("Global", out JsonElement globalProperty)) {
            _globalLevels = GetLevels(globalProperty);
        }

        _handlerConfigs = new();

        foreach (JsonProperty configProp in document.RootElement.EnumerateObject()) {

            if (configProp.Name == "Default")
                continue;

            LogConfigLevels levels = GetLevels(configProp.Value);

            bool enabled = true;
            if (configProp.Value.TryGetProperty("Enabled", out JsonElement enabledElement))
                enabled = enabledElement.GetBoolean();

            JsonElement settings = default;
            if (configProp.Value.TryGetProperty("Settings", out JsonElement settingsElement))
                settings = settingsElement;

            _handlerConfigs[configProp.Name] = new(configProp.Name, enabled, levels, settings.AsNode() as JsonObject);
        }
    }

    public LogConfigLevels GetGlobalLevels() => _globalLevels;

    public LogHandlerConfig GetHandlerConfig(string name) {
        if (!_handlerConfigs.TryGetValue(name, out LogHandlerConfig? value))
            value = new(name, true, LogConfigLevels.Default, default);
        return value;
    }
}

