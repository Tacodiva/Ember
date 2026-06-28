
using System;
using System.ComponentModel;

namespace Ember.Logging;

public enum LogLevel {
    Diagnostic = 0,
    Fine = 1,
    Info = 2,
    Minor = 3,
    Warning = 4,
    Error = 5,
    Debug = 6,
}

public static class LogLevelExt {
    public static ConsoleColor GetConsoleColor(this LogLevel level) {
        return level switch {
            LogLevel.Diagnostic => ConsoleColor.DarkGray,
            LogLevel.Fine => ConsoleColor.Gray,
            LogLevel.Info => ConsoleColor.White,
            LogLevel.Minor => ConsoleColor.DarkYellow,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Debug => ConsoleColor.Magenta,
            _ => throw new InvalidEnumArgumentException(nameof(level))
        };
    }

    public static string GetName(this LogLevel level) {
        return level switch {
            LogLevel.Diagnostic => "Diagnostic",
            LogLevel.Fine => "Fine",
            LogLevel.Info => "Info",
            LogLevel.Minor => "Minor",
            LogLevel.Warning => "Warning",
            LogLevel.Error => "Error",
            LogLevel.Debug => "Debug",
            _ => throw new InvalidEnumArgumentException(nameof(level))
        };
    }

    public static string GetShortName(this LogLevel level) {
        return level switch {
            LogLevel.Diagnostic => "Diag",
            LogLevel.Fine => "Fine",
            LogLevel.Info => "Info",
            LogLevel.Minor => "Minor",
            LogLevel.Warning => "Warn",
            LogLevel.Error => "Err",
            LogLevel.Debug => "Debug",
            _ => throw new InvalidEnumArgumentException(nameof(level))
        };
    }
}