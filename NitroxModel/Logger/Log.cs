global using NitroxModel.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using NitroxModel.Helper;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;

namespace NitroxModel.Logger
{
    public static class Log
    {
        private static ILogger logger = Serilog.Core.Logger.None;
        private static readonly HashSet<int> logOnceCache = new();
        private static bool isSetup;

        public static string PlayerName
        {
            set => SetPlayerName(value);
        }

        public static string LogDirectory { get; } = Path.GetFullPath(Path.Combine(NitroxUser.LauncherPath ?? "", "Nitrox Logs"));

        public static string GetMostRecentLogFile() => new DirectoryInfo(LogDirectory).GetFiles().OrderByDescending(f => f.CreationTimeUtc).FirstOrDefault()?.FullName;

        public static void Setup(bool asyncConsoleWriter = false, InGameLogger inGameLogger = null, bool isConsoleApp = false, bool useConsoleLogging = true)
        {
            if (isSetup)
            {
                throw new Exception($"{nameof(Log)} setup should only be executed once.");
            }
            isSetup = true;
            
            PlayerName = "";
            logger = new LoggerConfiguration()
                     .MinimumLevel.Debug()
                     .WriteTo.Logger(cnf =>
                     {
                         if (!useConsoleLogging)
                         {
                             return;
                         }

                         string consoleTemplate = isConsoleApp switch
                         {
                             false => $"[{{Timestamp:HH:mm:ss.fff}}] {{{nameof(PlayerName)}:l}}[{{Level:u3}}] {{Message}}{{NewLine}}{{Exception}}",
                             _ => "[{Timestamp:HH:mm:ss.fff}] {Message}{NewLine}{Exception}"
                         };

                         if (asyncConsoleWriter)
                         {
                             cnf.WriteTo.Async(a => a.ColoredConsole(outputTemplate: consoleTemplate));
                         }
                         else
                         {
                             cnf.WriteTo.ColoredConsole(outputTemplate: consoleTemplate);
                         }
                     })
                     .WriteTo.Logger(cnf => cnf
                                            .Enrich.FromLogContext()
                                            .WriteTo
#if DEBUG
                                            .Map(nameof(PlayerName), "", (playerName, sinkCnf) => sinkCnf.Async(a => a.File(Path.Combine(LogDirectory, $"{GetLogFileName()}{playerName}-.log"),
#else
                                            .Async((a => a.File(Path.Combine(LogDirectory, $"{GetLogFileName()}-.log"),
#endif
                                                                                                                            outputTemplate: "[{Timestamp:HH:mm:ss.fff}] [{Level:u3}{IsUnity}] {Message}{NewLine}{Exception}",
                                                                                                                            rollingInterval: RollingInterval.Day,
                                                                                                                            retainedFileCountLimit: 10,
                                                                                                                            fileSizeLimitBytes: 200000000, // 200MB
                                                                                                                            shared: true))))
                     .WriteTo.Logger(cnf =>
                     {
                         if (inGameLogger == null)
                         {
                             return;
                         }
                         cnf
                             .Enrich.FromLogContext()
                             .WriteTo.Conditional(evt => evt.Properties.ContainsKey("game"), configuration => configuration.Message(inGameLogger.Log));
                     })
                     .CreateLogger();
        }

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

        public static void InGame(object message)
        {
            InGame(message?.ToString());
        }

        public static void InGame(string message)
        {
            using (LogContext.PushProperty("game", true))
            {
                logger.Information(message);
            }
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

        public static void InGameSensitive(string message, params object[] args)
        {
            using (LogContext.Push(SensitiveEnricher.Instance))
            using (LogContext.PushProperty("game", true))
            {
                logger.Information(message, args);
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
            if (string.IsNullOrEmpty(value))
            {
                LogContext.PushProperty(nameof(PlayerName), "");
                return;
            }

            if (logger != null)
            {
                Info($"Setting player name to {value}");
            }
            LogContext.PushProperty(nameof(PlayerName), @$"[{value}]");
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
    }

    public enum LogLevel
    {
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4
    }
}
