global using NitroxModel.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using LiteNetLib;
using NitroxModel.Helper;
using Serilog;
using Serilog.Configuration;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;

namespace NitroxModel.Logger
{
    public static class Log
    {
        private static ILogger logger = Serilog.Core.Logger.None;
        private static ILogger inGameLogger = Serilog.Core.Logger.None;
        private static readonly HashSet<int> logOnceCache = new();
        private static bool isSetup;

        public static string PlayerName
        {
            set => SetPlayerName(value);
        }

        public static string SaveName
        {
            set => SetSaveName(value);
        }

        private static string LogDecorator { get; set; }

        public static string LogDirectory { get; } = Path.GetFullPath(Path.Combine(NitroxUser.LauncherPath ?? "", "Nitrox Logs"));

        public static string GetMostRecentLogFile() => new DirectoryInfo(LogDirectory).GetFiles().OrderByDescending(f => f.CreationTimeUtc).FirstOrDefault()?.FullName;

        public static void Setup(bool asyncConsoleWriter = false, InGameLogger gameLogger = null, bool isConsoleApp = false, bool useConsoleLogging = true, bool useFileLogging = true)
        {
            if (isSetup)
            {
                Warn($"{nameof(Log)} setup should only be executed once.");
                return;
            }

            isSetup = true;
            NetDebug.Logger = new LiteNetLibLogger();

            PlayerName = "";
            SaveName = "";

            // Configure logger and create an instance of it.
            LoggerConfiguration loggerConfig = new LoggerConfiguration().MinimumLevel.Debug();
            if (useConsoleLogging)
            {
                loggerConfig = loggerConfig.WriteTo.AppendConsoleSink(asyncConsoleWriter, isConsoleApp);
            }
            if (useFileLogging)
            {
                loggerConfig = loggerConfig.WriteTo.AppendFileSink();
            }
            logger = loggerConfig.CreateLogger();

            if (gameLogger != null)
            {
                inGameLogger = new LoggerConfiguration()
                               .WriteTo.Logger(cnf => cnf.Enrich.FromLogContext().WriteTo.Message(gameLogger.Log))
                               .CreateLogger();
            }
        }

        private static LoggerConfiguration AppendFileSink(this LoggerSinkConfiguration sinkConfig) => sinkConfig.Logger(cnf =>
        {
            cnf.Enrich.FromLogContext()
               .WriteTo
               .Valve(v =>
               {
                   v.Async(a =>
                   {
                       a.Map(nameof(LogDecorator), "", (logDecorator, sinkCnf) =>
                       {
                           sinkCnf.File(Path.Combine(LogDirectory, $"{GetLogFileName()}{logDecorator}-.log"),
                                        outputTemplate: "[{Timestamp:HH:mm:ss.fff}] [{Level:u3}{IsUnity}] {Message}{NewLine}{Exception}",
                                        rollingInterval: RollingInterval.Day,
                                        retainedFileCountLimit: 10,
                                        fileSizeLimitBytes: 200_000_000, // 200MB
                                        shared: true);
                       });
                   });
               }, e => e.Properties.TryGetValue(nameof(SaveName), out LogEventPropertyValue propertyValue) && !string.IsNullOrWhiteSpace(propertyValue.ToString()));
        });

        private static LoggerConfiguration AppendConsoleSink(this LoggerSinkConfiguration sinkConfig, bool makeAsync, bool useShorterTemplate) => sinkConfig.Logger(cnf =>
        {
            string consoleTemplate = useShorterTemplate switch
            {
                false => $"[{{Timestamp:HH:mm:ss.fff}}] {{{nameof(PlayerName)}:l}}[{{Level:u3}}] {{Message}}{{NewLine}}{{Exception}}",
                _ => "[{Timestamp:HH:mm:ss.fff}] {Message}{NewLine}{Exception}"
            };

            if (makeAsync)
            {
                cnf.WriteTo.Async(a => a.ColoredConsole(outputTemplate: consoleTemplate));
            }
            else
            {
                cnf.WriteTo.ColoredConsole(outputTemplate: consoleTemplate);
            }
        });

        [Conditional("DEBUG")]
        public static void Debug(string message)
        {
            logger.Debug(message);
        }

        [Conditional("DEBUG")]
        public static void Debug(object message)
        {
            Debug(message?.ToString());
        }

        public static void Info(string message)
        {
            logger.Information(message);
        }

        public static void Info(object message)
        {
            Info(message?.ToString());
        }

        public static void Warn(string message)
        {
            logger.Warning(message);
        }

        public static void Warn(object message)
        {
            Warn(message?.ToString());
        }

        public static void Error(Exception ex)
        {
            logger.Error(ex, ex.Message);
        }

        public static void Error(Exception ex, string message)
        {
            logger.Error(ex, message);
        }

        public static void Error(string message)
        {
            logger.Error(message);
        }

        /// <summary>
        ///     Only logs the message one time. The messages must be the same for this function to work.
        /// </summary>
        public static void WarnOnce(string message)
        {
            int hash = message?.GetHashCode() ?? 0;
            if (logOnceCache.Contains(hash))
            {
                return;
            }

            Warn(message);
            logOnceCache.Add(hash);
        }

        public static void ErrorOnce(string message)
        {
            int hash = message?.GetHashCode() ?? 0;
            if (logOnceCache.Add(hash))
            {
                Error(message);
            }
        }

        public static void Verbose(string message)
        {
            Write(LogLevel.Verbose, message);
        }

        public static void InGame(string message)
        {
            inGameLogger.Information(message);
        }

        public static void Write(LogLevel level, string message)
        {
            logger.Write((LogEventLevel)level, message);
        }

        [Conditional("DEBUG")]
        public static void DebugSensitive(string message, params object[] args)
        {
            using (LogContext.Push(SensitiveEnricher.Instance))
            {
                logger.Debug(message, args);
            }
        }

        public static void InfoSensitive(string message, params object[] args)
        {
            using (LogContext.Push(SensitiveEnricher.Instance))
            {
                logger.Information(message, args);
            }
        }

        public static void WarnSensitive(string message, params object[] args)
        {
            using (LogContext.Push(SensitiveEnricher.Instance))
            {
                logger.Warning(message, args);
            }
        }

        public static void ErrorSensitive(Exception ex, string message, params object[] args)
        {
            using (LogContext.Push(SensitiveEnricher.Instance))
            {
                logger.Error(ex, message, args);
            }
        }

        public static void ErrorSensitive(string message, params object[] args)
        {
            using (LogContext.Push(SensitiveEnricher.Instance))
            {
                logger.Error(message, args);
            }
        }

        public static void ErrorUnity(string message)
        {
            using (LogContext.PushProperty("IsUnity", "-UNITY"))
            {
                logger.Error(message);
            }
        }

        // Player name in log file is only important with running two instances of Nitrox.
        [Conditional("DEBUG")]
        private static void SetPlayerName(string value)
        {
#if DEBUG
            if (string.IsNullOrEmpty(value))
            {
                LogContext.PushProperty(nameof(LogDecorator), "");
                return;
            }
            
            LogContext.PushProperty(nameof(LogDecorator), @$"[{value}]");
#endif
            if (logger != null)
            {
                Info($"Setting player name to {value}");
            }
        }
        
        private static void SetSaveName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                LogContext.PushProperty(nameof(LogDecorator), "");
                return;
            }

            LogContext.PushProperty(nameof(LogDecorator), @$"[{value}]");
        }

        /// <summary>
        ///     Get log file friendly name of the application that is currently logging.
        /// </summary>
        /// <returns>Friendly display name of the current application.</returns>
        private static string GetLoggerName()
        {
            string name = Assembly.GetEntryAssembly()?.GetName().Name ?? "Client"; // Unity Engine does not set Assembly name so lets default to 'Client'.
            return name.IndexOf("server", StringComparison.InvariantCultureIgnoreCase) >= 0 ? "Server" : name;
        }

        private static string GetLogFileName()
        {
            static bool Contains(string haystack, string needle) => haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;

            string loggerName = GetLoggerName();
            if (Contains(loggerName, "server"))
            {
                return "server";
            }

            if (Contains(loggerName, "launch"))
            {
                return "launcher";
            }

            return "game";
        }

        private class SensitiveEnricher : ILogEventEnricher
        {
            /// <summary>
            ///     Parameters that are being logged with these names should be excluded when a log was made through the sensitive
            ///     method calls.
            /// </summary>
            private static readonly HashSet<string> sensitiveLogParameters = new HashSet<string>
            {
                "username",
                "password",
                "ip",
                "hostname",
                "path"
            };

            private static readonly Lazy<SensitiveEnricher> instance = new Lazy<SensitiveEnricher>(() => new SensitiveEnricher(), LazyThreadSafetyMode.PublicationOnly);
            public static SensitiveEnricher Instance => instance.Value;

            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propFactory)
            {
                foreach ((string key, string value) prop in GetPropertiesAsRedacted(logEvent.Properties.ToArray()))
                {
                    logEvent.AddOrUpdateProperty(propFactory.CreateProperty(prop.key, prop.value));
                }
            }

            private IEnumerable<(string key, string value)> GetPropertiesAsRedacted(IEnumerable<KeyValuePair<string, LogEventPropertyValue>> originalProps)
            {
                foreach (KeyValuePair<string, LogEventPropertyValue> prop in originalProps)
                {
                    if (!sensitiveLogParameters.Contains(prop.Key))
                    {
                        continue;
                    }

                    yield return (prop.Key, new string('*', prop.Value.ToString().Length));
                }
            }
        }
        
        public static LoggerConfiguration Valve(
            this LoggerSinkConfiguration loggerConfiguration,
            Action<LoggerSinkConfiguration> configure,
            Func<LogEvent, bool> predicate,
            LogEventLevel minimumLevel = LogEventLevel.Verbose,
            string outputTemplate = "{Message}",
            IFormatProvider formatProvider = null)
        {
            return LoggerSinkConfiguration.Wrap(loggerConfiguration, wrappedSink => new ConditionalValveSink(predicate, wrappedSink), configure, LogEventLevel.Verbose, null);
        }
    }

    public enum LogLevel
    {
        Verbose = 0,
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4
    }
}
