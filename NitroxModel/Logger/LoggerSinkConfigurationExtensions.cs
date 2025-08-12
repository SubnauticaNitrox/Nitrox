using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace NitroxModel.Logger;

public static class LoggerSinkConfigurationExtensions
{
    /// <summary>
    ///     Buffers messages while the <see cref="Predicate{T}"/> returns true. If false, resumes buffering.
    /// </summary>
    public static LoggerConfiguration Valve(
        this LoggerSinkConfiguration loggerConfiguration,
        Action<LoggerSinkConfiguration> configure,
        Func<LogEvent, bool> predicate,
        LogEventLevel minimumLevel = LogEventLevel.Verbose)
    {
        ILogEventSink logEventSink = LoggerSinkConfiguration.Wrap(wrappedSink => new ConditionalValveSink(predicate, wrappedSink), configure);
        return loggerConfiguration.Sink(logEventSink, minimumLevel);
    }

    public static LoggerConfiguration Message(
        this LoggerSinkConfiguration loggerConfiguration,
        Action<string> writer,
        LogEventLevel minimumLevel = LogEventLevel.Verbose,
        string outputTemplate = "{Message}",
        IFormatProvider formatProvider = null)
    {
        return loggerConfiguration.Sink(new MessageSink(new MessageTemplateTextFormatter(outputTemplate, formatProvider), writer), minimumLevel);
    }
}
