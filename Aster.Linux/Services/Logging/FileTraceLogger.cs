using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace Aster.Linux.Services.Logging;

internal sealed class FileTraceLogger : IDisposable
{
    private readonly TraceListener? _listener;

    private FileTraceLogger(TraceListener? listener) => _listener = listener;

    public static FileTraceLogger Initialize()
    {
        List<string> directories = [];
        string? stateHome = Environment.GetEnvironmentVariable("XDG_STATE_HOME");
        if (!string.IsNullOrWhiteSpace(stateHome) && Path.IsPathRooted(stateHome))
            directories.Add(Path.Combine(stateHome, "Aster"));

        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(home))
            directories.Add(Path.Combine(home, ".local", "state", "Aster"));

        directories.Add(Path.Combine(AppContext.BaseDirectory, "logs"));
        directories.Add(Path.Combine(Path.GetTempPath(), "Aster", "logs"));

        foreach (string directory in directories)
        {
            try
            {
                Directory.CreateDirectory(directory);
                StreamWriter writer = new(
                    new FileStream(
                        Path.Combine(directory, "aster.log"),
                        FileMode.Append,
                        FileAccess.Write,
                        FileShare.ReadWrite
                    ),
                    new UTF8Encoding(false)
                )
                {
                    AutoFlush = true,
                };
                TraceListener listener = new TimestampTraceListener(writer);
                Trace.Listeners.Add(listener);
                Trace.AutoFlush = true;
                Trace.TraceInformation("Aster log started. File: {0}", Path.Combine(directory, "aster.log"));
                return new FileTraceLogger(listener);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(
                    $"Unable to create Aster log in '{directory}': {exception.Message}"
                );
            }
        }

        Console.Error.WriteLine("Aster file logging is unavailable.");
        return new FileTraceLogger(null);
    }

    public void Dispose()
    {
        if (_listener is null)
            return;

        Trace.TraceInformation("Aster log stopped.");
        Trace.Flush();
        Trace.Listeners.Remove(_listener);
        _listener.Dispose();
    }

    private sealed class TimestampTraceListener(TextWriter writer) : TraceListener
    {
        private readonly object _sync = new();
        private bool _lineOpen;

        public override void Write(string? message)
        {
            lock (_sync)
            {
                WriteTimestampIfNeeded();
                writer.Write(Sanitize(message));
                _lineOpen = true;
            }
        }

        public override void WriteLine(string? message)
        {
            lock (_sync)
            {
                WriteTimestampIfNeeded();
                writer.WriteLine(Sanitize(message));
                writer.Flush();
                _lineOpen = false;
            }
        }

        public override void TraceEvent(
            TraceEventCache? eventCache,
            string? source,
            TraceEventType eventType,
            int id,
            string? message
        )
        {
            lock (_sync)
            {
                if (_lineOpen)
                    writer.WriteLine();

                writer.WriteLine(
                    $"{DateTimeOffset.Now.ToString("O", CultureInfo.InvariantCulture)} [{eventType}] {source} ({id}): {Sanitize(message)}"
                );
                writer.Flush();
                _lineOpen = false;
            }
        }

        private void WriteTimestampIfNeeded()
        {
            if (!_lineOpen)
                writer.Write($"{DateTimeOffset.Now.ToString("O", CultureInfo.InvariantCulture)} [TRACE] ");
        }

        private static string? Sanitize(string? message) => message?.Replace("\0", string.Empty);
    }
}
