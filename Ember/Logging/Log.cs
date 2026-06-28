using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Text.Json;
using Ember.Utils;
using Json.Schema;
using System.Threading;

namespace Ember.Logging;

public static class Log {

    [ThreadStatic]
    private static StringBuilder? _threadStringBuilder;
    internal static StringBuilder ThreadStringBuilder {
        get {
            if (_threadStringBuilder == null) _threadStringBuilder = new();
            else _threadStringBuilder.Clear();
            return _threadStringBuilder;
        }
    }

    private static readonly Lock _lock = new();

    private record struct ConfigLevelCacheEntry(
        bool HasLevel,
        LogLevel Level
    );

    private class LogHandlerEntry {
        public readonly string Name;
        public ILogHandler Handler;

        public LogHandlerConfig Config;
        public Dictionary<string, ConfigLevelCacheEntry> ConfigLevelCache;

        public LogHandlerEntry(ILogHandler handler, LogHandlerConfig config) {
            Config = config;
            Handler = handler;
            Name = config.Name;
            ConfigLevelCache = new();
            Handler.UpdateSettings(Config.Settings);
        }
    }

    private static List<LogHandlerEntry> _logHandlers = new();

    // All set by SetConfig in the static constructor.
    public static ILogConfig Config { get; private set; } = null!;
    private static LogConfigLevels _globalLevels = null!;

    private static Dictionary<string, LogLevel>? _loggerLevelOverrides = null;

    static Log() {
        SetConfig(DefaultLogConfig.Instance);
        AddHandler(new ConsoleLogHandler());
    }

    private const int MaxObjectDepth = 4;
    private static string StringifyObject(object? obj, bool decorate = false, int depth = 0) {
        if (obj == null) return "null";
        if (obj is string str) {
            if (decorate) return $"\"{str}\"";
            else return str;
        }
        if (obj is ICollection collection) {
            StringBuilder sb = new(obj.GetType().Name);
            if (depth < MaxObjectDepth) {
                sb.Append(" { ");
                bool first = true;
                foreach (object item in collection) {
                    if (first) first = false;
                    else sb.Append(", ");
                    sb.Append(StringifyObject(item, true, depth + 1));
                }
                sb.Append(" }");
            } else {
                sb.Append(" { ... }");
            }
            return sb.ToString();
        }
        return obj.ToString() ?? "\"null\"";
    }

    internal static void WriteLog(Logger? logger, object? content, LogLevel level) {
        try {
            LogMessage message = new LogMessage(logger, StringifyObject(content), level);

            lock (_lock) {
                foreach (LogHandlerEntry handlerEntry in _logHandlers) {
                    if (handlerEntry.Config.Enabled && handlerEntry.Handler != null) {
                        ILogHandler handler = handlerEntry.Handler;

                        LogLevel minLevel;

                        if (logger == null) {
                            minLevel = handlerEntry.Config.Levels.DefaultLevel;
                        } else {

                            static bool TryGetMinFromConfig(Logger? logger, Dictionary<string, LogLevel> levels, out LogLevel level) {
                                while (logger != null) {
                                    if (levels.TryGetValue(logger.Path, out level))
                                        return true;
                                    logger = logger.Parent;
                                }
                                level = default;
                                return false;
                            }

                            if (_loggerLevelOverrides == null || !TryGetMinFromConfig(logger, _loggerLevelOverrides, out minLevel)) {
                                if (!handlerEntry.ConfigLevelCache.TryGetValue(logger.Path, out ConfigLevelCacheEntry cacheEntry)) {

                                    if (
                                        TryGetMinFromConfig(logger, handlerEntry.Config.Levels.LoggerLevels, out minLevel) ||
                                        TryGetMinFromConfig(logger, _globalLevels.LoggerLevels, out minLevel)
                                    ) {
                                        cacheEntry = new(true, minLevel);
                                    } else {
                                        cacheEntry = new(false, default);
                                    }

                                    handlerEntry.ConfigLevelCache[logger.Path] = cacheEntry;
                                }

                                if (cacheEntry.HasLevel) {
                                    minLevel = cacheEntry.Level;
                                } else if (logger.DefaultMinimum != null) {
                                    minLevel = logger.DefaultMinimum.Value;
                                } else {
                                    minLevel = handlerEntry.Config.Levels.DefaultLevel;
                                }
                            }
                        }

                        if (level < minLevel) continue;

                        handler.Handle(message);
                    }
                }
            }
        } catch (Exception e) {
            Console.Error.WriteLine("[WARN] Error while logging.");
            Console.Error.WriteLine(e);
        }
    }

    public static void WriteLog(LogMessage message) {
        WriteLog(message.Source, message.Text, message.Level);
    }

    public static void SetConfig(ILogConfig config) {
        lock (_lock) {
            Config = config;

            _globalLevels = config.GetGlobalLevels();

            foreach (LogHandlerEntry entry in _logHandlers) {
                LogHandlerConfig entryConfig = config.GetHandlerConfig(entry.Name);

                entry.Config = entryConfig;
                entry.ConfigLevelCache.Clear();
                entry.Handler.UpdateSettings(entry.Config.Settings);
            }
        }
    }

    private static LogHandlerEntry? GetHandlerEntry(string name) {
        lock (_lock) return _logHandlers.Find(handler => handler.Name == name);
    }

    public static ILogHandler? GetHandler(string name) {
        lock (_lock) return GetHandlerEntry(name)?.Handler;
    }

    public static void AddHandler(ILogHandler handler) {
        lock (_lock) {
            LogHandlerEntry? entry = GetHandlerEntry(handler.Name);

            if (entry != null)
                throw new InvalidOperationException($"Already have a log handler called '{handler.Name}'.");

            entry = new LogHandlerEntry(handler, Config.GetHandlerConfig(handler.Name));
            _logHandlers.Add(entry);
        }
    }

    public static void RemoveHandler(string name) {
        lock (_lock) {
            LogHandlerEntry? entry = GetHandlerEntry(name);
            if (entry == null) throw new InvalidOperationException($"No log handler called '{name}'.");
            _logHandlers.Remove(entry);
        }
    }

    public static void SetOverrideLevel(string path, LogLevel? minimum) {
        lock (_lock) {
            if (minimum == null) {
                if (_loggerLevelOverrides == null) return;
                if (_loggerLevelOverrides.Remove(path)) {
                    if (_loggerLevelOverrides.Count == 0)
                        _loggerLevelOverrides = null;
                }
            } else {
                _loggerLevelOverrides ??= new();
                _loggerLevelOverrides[path] = minimum.Value;
            }
        }
    }

    public static Logger CreateLogger(string prefix, LogLevel? defaultMinimum = null) {
        return new Logger(prefix, defaultMinimum, null);
    }

    public static void Info(object? log) {
        WriteLog(null, log, LogLevel.Info);
    }

    public static void Warn(object? log) {
        WriteLog(null, log, LogLevel.Warning);
    }

    public static void Warn(object? log, Exception e) {
        WriteLog(null, log + "\n" + e, LogLevel.Warning);
    }

    public static void WarnTrace(object? log) {
        WriteLog(null, log + "\n" + new StackTrace(1, RuntimeInfo.IsDebug), LogLevel.Warning);
    }

    public static void Minor(object? log) {
        WriteLog(null, log, LogLevel.Minor);
    }

    public static void Minor(object? log, Exception e) {
        WriteLog(null, log + "\n" + e, LogLevel.Minor);
    }

    public static void MinorTrace(object? log) {
        WriteLog(null, log + "\n" + new StackTrace(1, RuntimeInfo.IsDebug), LogLevel.Minor);
    }

    public static void Error(object? log) {
        WriteLog(null, log, LogLevel.Error);
    }

    public static void Error(object? log, Exception e) {
        WriteLog(null, log + "\n" + e, LogLevel.Error);
    }

    public static void ErrorTrace(object? log) {
        WriteLog(null, log + "\n" + new StackTrace(1, RuntimeInfo.IsDebug), LogLevel.Error);
    }

    public static void Fine(object? log) {
        WriteLog(null, log, LogLevel.Fine);
    }

    public static void Diagnostic(object? log) {
        WriteLog(null, log, LogLevel.Diagnostic);
    }

    [Conditional("DEBUG")]
    public static void Debug(object? log) {
        WriteLog(null, log, LogLevel.Debug);
    }

    [Conditional("DEBUG")]
    public static void DebugTrace(object? log) {
        WriteLog(null, log + "\n" + new StackTrace(1, RuntimeInfo.IsDebug), LogLevel.Debug);
    }
}