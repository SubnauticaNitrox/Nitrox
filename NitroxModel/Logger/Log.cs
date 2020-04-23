using System;
using System.Collections.Generic;
using System.Diagnostics;
using NLog;
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
                throw new Exception($"{nameof(Log)} setup should only be executed once.");
            }
            logger = LogManager.GetCurrentClassLogger();

            LoggingConfiguration config = new LoggingConfiguration();
            string layout = @"${date:format=HH\:mm\:ss.fff} [${level:uppercase=true}] ${event-properties:item=PlayerName}${message} ${exception}";

            // Targets where to log to: File and Console
            ColoredConsoleTarget logConsole = new ColoredConsoleTarget(nameof(logConsole))
            {
                Layout = layout,
                DetectConsoleAvailable = true
            };
            FileTarget logFile = new FileTarget(nameof(logFile))
            {
                FileName = "Nitrox Logs/nitrox.log",
                ArchiveFileName = "Nitrox Logs/archives/nitrox.{#}.log",
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveNumbering = ArchiveNumberingMode.Date,
                MaxArchiveFiles = 7,
                Layout = layout,
                EnableArchiveFileCompression = true,
            };
            AsyncTargetWrapper logfileAsync = new AsyncTargetWrapper(logFile, 1000, AsyncTargetWrapperOverflowAction.Grow);

            // Rules for mapping loggers to targets
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logConsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfileAsync);
            config.AddRuleForOneLevel(LogLevel.Info,
                                      new MethodCallTarget("ingame",
                                                           (evt, obj) =>
                                                           {
                                                               if (InGameLogger == null)
                                                               {
                                                                   return;
                                                               }
                                                               object isGameLog;
                                                               evt.Properties.TryGetValue("game", out isGameLog);
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
            logger.Info("Setting player name");
            if (playerName != null)
            {
                logger.SetProperty("PlayerName", string.Format($"{playerName} "));
            }
        }

        [Conditional("DEBUG")]
        public static void Trace(object message, params object[] args)
        {
            logger.Trace(message?.ToString(), args);
        }

        [Conditional("DEBUG")]
        public static void Debug(object message, params object[] args)
        {
            logger.Debug(message?.ToString(), args);
        }

        public static void Info(object message, params object[] args)
        {
            logger.Info(message?.ToString(), args);
        }

        public static void Warn(object message, params object[] args)
        {
            logger.Warn(message?.ToString(), args);
        }

        public static void Error(object message, params object[] args)
        {
            logger.Error(message?.ToString(), args);
        }

        public static void Error(Exception ex)
        {
            logger.Error(ex);
        }

        public static void Error(Exception ex, string message, params object[] args)
        {
            logger.Error(ex, message, args);
        }

        public static void Fatal(object message, params object[] args)
        {
            logger.Fatal().Message(message?.ToString(), args).Write();
        }

        [Conditional("DEBUG")]
        public static void DebugSensitive(object message, params object[] args)
        {
            logger
                .WithProperty("sensitive", true)
                .Debug()
                .Message(message?.ToString(), args)
                .Write();
        }

        public static void InfoSensitive(object message, params object[] args)
        {
            logger
                .WithProperty("sensitive", true)
                .Info()
                .Message(message?.ToString(), args)
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

        public static void ErrorSensitive(object message, params object[] args)
        {
            logger
                .WithProperty("sensitive", true)
                .Error()
                .Message(message?.ToString(), args)
                .Write();
        }

        public static void InGame(object message, bool containsPersonalInfo = false, params object[] args)
        {
            if (InGameLogger == null)
            {
                logger.Warn($"{nameof(InGameLogger)} has not been set.");
                return;
            }
            logger
                .WithProperty("sensitive", containsPersonalInfo)
                .WithProperty("game", true)
                .Info()
                .Message(message?.ToString(), args)
                .Write();
        }

        /// <summary>
        ///     Exclude sensitive logs parameters from being logged into (long-term) files
        /// </summary>
        /// <param name="config">The logger config to apply the filter to.</param>
        /// <param name="applyDecider">Custom condition to decide whether to apply the sensitive log file to a log target.</param>
        private static void AddSensitiveFilter(LoggingConfiguration config, Func<Target, bool> applyDecider)
        {
            WhenMethodFilter sensistiveLogFilter = new WhenMethodFilter(context =>
            {
                object isSensitive;
                context.Properties.TryGetValue("sensitive", out isSensitive);
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
                        rule.Filters.Add(sensistiveLogFilter);
                    }
                }
            }
        }
    }
}
