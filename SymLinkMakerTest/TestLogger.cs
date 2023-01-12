using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Globalization;

#nullable enable

namespace SymLinkMaker.Test
{
    /// <summary>
    ///     Basic logger for tests. Outputs to files. Not multithreaded. 
    ///     For compatibility between .NET versions.
    /// </summary>
    public class TestLogger : ILogger
    {
        LogLevel level;
        FileStream logFile;
        
        public TestLogger(string logLoc, LogLevel level)
        {
            if (!Directory.Exists(logLoc)) Directory.CreateDirectory(logLoc);
            logFile = File.Create(Path.Combine(logLoc, (DateTime.UtcNow.Ticks/10000).ToString() + ".log"));
            this.level = level;
        }

        public void Log<TState>(
            LogLevel logLevel, 
            EventId eventId, 
            TState state, 
            Exception? exception, 
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            string log = formatter(state, exception);

            string levelStr = "";

            switch (logLevel) {
                case LogLevel.Information:
                    levelStr = "INFO";
                    break;
                case LogLevel.Debug:
                    levelStr = "DEBUG";
                    break;
                case LogLevel.Trace:
                    levelStr = "TRACE";
                    break;
                case LogLevel.Warning:
                    levelStr = "WARNING";
                    break;
                case LogLevel.Error:
                    levelStr = "ERROR";
                    break;
                case LogLevel.Critical:
                    levelStr = "CRITICAL";
                    break;
            }

            string timeStr = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);

            logFile.Write(Encoding.UTF8.GetBytes($"{levelStr} [{timeStr}]: {log}\n"));
        }

        public bool IsEnabled(LogLevel level)
        {
            return level >= this.level && level != LogLevel.None;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

        public void LogDebug(string? msg, params object?[] args)
        {
            Log(
                LogLevel.Debug, 
                new EventId(0), 
                string.Format("DEBUG: " + msg, args), 
                null, 
                (string s, Exception? ex) => { return s; }
            );
        }

        public void LogTrace(string? msg, params object?[] args)
        {
            Log(
                LogLevel.Trace, 
                new EventId(0),
                string.Format("TRACE: " + msg, args),
                null,
                (string s, Exception? ex) => { return s; }
            );
        }

        public void LogInformation(string? msg, params object?[] args)
        {
            Log(
                LogLevel.Information, 
                new EventId(0),
                string.Format("INFO: " + msg, args),
                null,
                (string s, Exception? ex) => { return s; }
            );
        }

        void Warning(string? message, params object?[] args)
        {
            Log(
                LogLevel.Warning, 
                new EventId(0),
                string.Format("WARNING: " + message, args),
                null,
                (string s, Exception? ex) => { return s; }
            );
        }

        public void LogError(string? msg, params object?[] args)
        {
            Log(
                LogLevel.Error, 
                new EventId(0),
                string.Format("ERROR: " + msg, args),
                null,
                (string s, Exception? ex) => { return s; }
            );
        }

        public void Finish()
        {
            logFile.Close();
        }
    }
}
