using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace NitroxModel.Logger
{
    public class ColoredConsoleSink : ILogEventSink
    {
        private readonly ConsoleColor defaultBackground = Console.BackgroundColor;
        private readonly ConsoleColor defaultForeground = Console.ForegroundColor;

        private readonly ITextFormatter formatter;

        public ColoredConsoleSink(ITextFormatter formatter)
        {
            this.formatter = formatter;
        }

        public void Emit(LogEvent logEvent)
        {
            (Console.ForegroundColor, Console.BackgroundColor) = logEvent.Level switch
            {
                LogEventLevel.Verbose => (ConsoleColor.DarkGray, defaultBackground),
                LogEventLevel.Debug => (ConsoleColor.DarkGray, defaultBackground),
                LogEventLevel.Information => (ConsoleColor.Gray, defaultBackground),
                LogEventLevel.Warning => (ConsoleColor.Yellow, defaultBackground),
                LogEventLevel.Error => (ConsoleColor.Red, defaultBackground),
                LogEventLevel.Fatal => (ConsoleColor.Red, ConsoleColor.Yellow),
                _ => (defaultForeground, defaultBackground)
            };

            formatter.Format(logEvent, Console.Out);
            Console.Out.Flush();

            Console.ForegroundColor = defaultForeground;
            Console.BackgroundColor = defaultBackground;
        }
    }

    public static class ColoredConsoleSinkExtensions
    {
        public static LoggerConfiguration ColoredConsole(
            this LoggerSinkConfiguration loggerConfiguration,
            LogEventLevel minimumLevel = LogEventLevel.Verbose,
            string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
            IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new ColoredConsoleSink(new MessageTemplateTextFormatter(outputTemplate, formatProvider)), minimumLevel);
        }
    }
}
