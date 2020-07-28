using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using NLog;
using NLog.Conditions;
using NLog.Config;
using NLog.Filters;
using NLog.Fluent;
using NLog.MessageTemplates;
using NLog.Targets;
using NLog.Targets.Wrappers;
using NLog.Time;

namespace NitroxModel.Logger
{
    public static class Log
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
            "hostname"
        };

        private static NLog.Logger logger;

        public static InGameLogger InGameLogger { private get; set; }

        public static void Setup(bool performanceCritical = false)
        {
            if (logger != null)
            {
                throw new InvalidOperationException($"{nameof(Log)} setup should only be executed once.");
            }
            logger = LogManager.GetCurrentClassLogger();

            LoggingConfiguration config = new LoggingConfiguration();
            string layout = $@"[${{date:format=HH\:mm\:ss}} {GetLoggerName()}${{event-properties:item=PlayerName}}][${{level:uppercase=true}}] ${{message}} ${{exception}}";

            // Targets where to log to: File and Console
            ColoredConsoleTarget logConsole = new ColoredConsoleTarget(nameof(logConsole))
            {
                Layout = layout,
                DetectConsoleAvailable = true
            };

            logConsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule
            {
                Condition = ConditionParser.ParseExpression("level == LogLevel.Error"),
                ForegroundColor = ConsoleOutputColor.Red
            });
            logConsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule
            {
                Condition = ConditionParser.ParseExpression("level == LogLevel.Warn"),
                ForegroundColor = ConsoleOutputColor.Yellow
            });
            logConsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule
            {
                Condition = ConditionParser.ParseExpression("level == LogLevel.Debug"),
                ForegroundColor = ConsoleOutputColor.DarkGray
            });

            FileTarget logFile = new FileTarget(nameof(logFile))
            {
                FileName = $"Nitrox Logs/Nitrox-{GetLoggerName()}.log",
                ArchiveFileName = $"Nitrox Logs/archives/Nitrox-{GetLoggerName()}.{{#}}.log",
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveNumbering = ArchiveNumberingMode.Date,
                MaxArchiveFiles = 7,
                Layout = layout,
                EnableArchiveFileCompression = true,
            };
            AsyncTargetWrapper logFileAsync = new AsyncTargetWrapper(logFile, 1000, AsyncTargetWrapperOverflowAction.Grow);

            // Rules for mapping loggers to targets
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logConsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logFileAsync);
            config.AddRuleForOneLevel(LogLevel.Info,
                                      new MethodCallTarget("ingame",
                                                           (evt, obj) =>
                                                           {
                                                               if (InGameLogger == null)
                                                               {
                                                                   return;
                                                               }
                                                               evt.Properties.TryGetValue("game", out object isGameLog);
                                                               if (isGameLog != null && (bool)isGameLog)
                                                               {
                                                                   InGameLogger.Log(evt.FormattedMessage);
                                                               }
                                                           }));

            AddSensitiveFilter(config, target => target is AsyncTargetWrapper || target is FileTarget);

            // Apply config
            LogManager.Configuration = config;
            if (!performanceCritical)
            {
                TimeSource.Current = new AccurateLocalTimeSource();
            }
        }

        public static void SetPlayerName(string playerName)
        {
#if DEBUG //Player name in log file is just important with two instances => Developer
            if (!string.IsNullOrEmpty(playerName))
            {
                logger.Info($"Setting player name to {playerName}");
                logger.SetProperty("PlayerName", $"-{playerName}");
            }
#endif
        }

        [Conditional("DEBUG")]
        public static void Debug(string message) => logger.Debug(message);
        [Conditional("DEBUG")]
        public static void Debug(object message) => Debug(message?.ToString());

        public static void Info(string message) => logger.Info(message);
        public static void Info(object message) => Info(message?.ToString());

        public static void Warn(string message) => logger.Warn(message);
        public static void Warn(object message) => Warn(message?.ToString());

        public static void Error(Exception ex) => logger.Error(ex);
        public static void Error(Exception ex, string message) => logger.Error(ex, message);
        public static void Error(string message) => logger.Error(message);

        public static void InGame(object message) => InGame(message?.ToString());
        public static void InGame(string message)
        {
            if (InGameLogger == null)
            {
                logger.Warn($"{nameof(InGameLogger)} has not been set.");
                return;
            }
            logger
                .WithProperty("game", true)
                .Info()
                .Message(message)
                .Write();
        }

        [Conditional("DEBUG")]
        public static void DebugSensitive(string message, params object[] args)
        {
            logger
                .WithProperty("sensitive", true)
                .Debug()
                .Message(message, args)
                .Write();
        }

        public static void InfoSensitive(string message, params object[] args)
        {
            logger
                .WithProperty("sensitive", true)
                .Info()
                .Message(message, args)
                .Write();
        }

        public static void ErrorSensitive(Exception ex, string message, params object[] args)
        {
            logger
                .WithProperty("sensitive", true)
                .Error()
                .Exception(ex)
                .Message(message, args)
                .Write();
        }

        public static void ErrorSensitive(string message, params object[] args)
        {
            logger
                .WithProperty("sensitive", true)
                .Error()
                .Message(message, args)
                .Write();
        }

        public static void InGameSensitive(string message, params object[] args)
        {
            if (InGameLogger == null)
            {
                logger.Warn($"{nameof(InGameLogger)} has not been set.");
                return;
            }
            logger
                .WithProperty("sensitive", true)
                .WithProperty("game", true)
                .Info()
                .Message(message, args)
                .Write();
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

        /// <summary>
        ///     Exclude sensitive logs parameters from being logged into (long-term) files
        /// </summary>
        /// <param name="config">The logger config to apply the filter to.</param>
        /// <param name="applyDecider">Custom condition to decide whether to apply the sensitive log file to a log target.</param>
        private static void AddSensitiveFilter(LoggingConfiguration config, Func<Target, bool> applyDecider)
        {
            WhenMethodFilter sensitiveLogFilter = new WhenMethodFilter(context =>
            {
                context.Properties.TryGetValue("sensitive", out object isSensitive);
                if (isSensitive != null && (bool)isSensitive)
                {
                    for (int i = 0; i < context.MessageTemplateParameters.Count; i++)
                    {
                        MessageTemplateParameter template = context.MessageTemplateParameters[i];
                        if (sensitiveLogParameters.Contains(template.Name))
                        {
                            context.Parameters.SetValue(new string('*', template.Value?.ToString().Length ?? 0), i);
                        }
                    }
                    context.Parameters = context.Parameters; // Triggers NLog to format the message again
                }
                return FilterResult.Log;
            });
            foreach (LoggingRule rule in config.LoggingRules)
            {
                foreach (Target target in rule.Targets)
                {
                    if (applyDecider(target))
                    {
                        rule.Filters.Add(sensitiveLogFilter);
                    }
                }
            }
        }
    }
}
