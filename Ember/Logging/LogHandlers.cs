
using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Ember.Logging;

public interface ILogHandler {
    public string Name { get; }

    public void UpdateSettings(JsonObject? settings) { }
    public void Handle(LogMessage message);
    public void Flush() { }
}

public class ConsoleLogHandler : ILogHandler {

    public string Name => "Console";
    public bool UseColor { get; private set; }

    public ConsoleLogHandler() {

    }

    public void Handle(LogMessage message) {
        if (UseColor) Console.ForegroundColor = message.Level.GetConsoleColor();
        Console.WriteLine(message.ToString());
        if (UseColor) Console.ResetColor();
    }

    public void UpdateSettings(JsonObject? settings) {
        UseColor = true;
        if (settings == null) return;

        if (settings.TryGetPropertyValue("UseColor", out JsonNode? useColorElement)) UseColor = ((bool)useColorElement!);
    }
}

public class FileLogHandler : ILogHandler, IDisposable {

    public string Name => "File";
    public readonly string DirectoryPath;
    public readonly string FilePath;
    public readonly bool DidCompression;

    private readonly StreamWriter _writer;

    public FileLogHandler(string folder, bool compress) {
        DirectoryPath = folder;
        DidCompression = compress;

        FilePath = Path.Join(DirectoryPath, DateTime.Now.ToFileTime() + ".log");
        _writer = File.AppendText(FilePath);

        DirectoryInfo directory = Directory.CreateDirectory(DirectoryPath);

        foreach (FileInfo fileInfo in directory.EnumerateFiles()) {
            if (fileInfo.Extension == ".log") {
                using (FileStream inFile = fileInfo.OpenRead()) {
                    using FileStream outFile = File.Create(fileInfo.FullName + ".gz");
                    using GZipStream compressionStream = new(outFile, CompressionMode.Compress);
                    inFile.CopyTo(compressionStream);
                }
                fileInfo.Delete();
            }
        }
    }

    public void Handle(LogMessage message) {
        _writer.WriteLine(message.ToString());
    }

    public void Flush() {
        _writer.Flush();
    }

    public void Dispose() {
        _writer.Dispose();
    }
}