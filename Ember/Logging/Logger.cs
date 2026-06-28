
using System;
using System.Diagnostics;

namespace Ember.Logging;

public class Logger {

    public readonly string Prefix;
    public readonly string Path;
    public readonly Logger? Parent;
    public readonly LogLevel? DefaultMinimum;

    internal Logger(string prefix, LogLevel? defaultMinimum, Logger? parent) {
        for (int i = 0; i < prefix.Length; i++) {
            if (!char.IsLetterOrDigit(prefix[i]) && prefix[i] != '_')
                throw new ArgumentException($"Illegal character '{prefix[i]}' in logger prefix '{prefix}'. Only alphanumeric characters or '_' are allowed.");
        }
        string path = prefix;
        prefix = $"[{prefix}]";
        if (parent != null) {
            prefix = parent.Prefix + prefix;
            path = parent.Path + "/" + path;
        }
        Prefix = prefix;
        Path = path;
        Parent = parent;
        DefaultMinimum = defaultMinimum;
    }

    public Logger CreateLogger(string prefix, LogLevel? defaultMinimum = null) {
        return new Logger(prefix, defaultMinimum ?? DefaultMinimum, this);
    }

    public void Info(object? log) {
        Log.WriteLog(this, log, LogLevel.Info);
    }

    public void Warn(object? log) {
        Log.WriteLog(this, log, LogLevel.Warning);
    }

    public void Warn(object? log, Exception e) {
        Log.WriteLog(this, log + "\n" + e, LogLevel.Warning);
    }

    public void WarnTrace(object? log) {
        Log.WriteLog(this, log + "\n" + new StackTrace(1, RuntimeInfo.IsDebug), LogLevel.Warning);
    }

    public void Minor(object? log) {
        Log.WriteLog(this, log, LogLevel.Minor);
    }

    public void Minor(object? log, Exception e) {
        Log.WriteLog(this, log + "\n" + e, LogLevel.Minor);
    }

    public void MinorTrace(object? log) {
        Log.WriteLog(this, log + "\n" + new StackTrace(1, RuntimeInfo.IsDebug), LogLevel.Minor);
    }

    public void Error(object? log) {
        Log.WriteLog(this, log, LogLevel.Error);
    }

    public void Error(object? log, Exception e) {
        Log.WriteLog(this, log + "\n" + e, LogLevel.Error);
    }

    public void ErrorTrace(object? log) {
        Log.WriteLog(this, log + "\n" + new StackTrace(1, RuntimeInfo.IsDebug), LogLevel.Error);
    }

    public void Fine(object? log) {
        Log.WriteLog(this, log, LogLevel.Fine);
    }

    public void Diagnostic(object? log) {
        Log.WriteLog(this, log, LogLevel.Diagnostic);
    }

    [Conditional("DEBUG")]
    public void Debug(object? log) {
        Log.WriteLog(this, log, LogLevel.Debug);
    }

    [Conditional("DEBUG")]
    public void DebugTrace(object? log) {
        Log.WriteLog(this, log + "\n" + new StackTrace(1, RuntimeInfo.IsDebug), LogLevel.Debug);
    }
}