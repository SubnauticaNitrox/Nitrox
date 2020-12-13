using System;
using System.IO;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace NitroxModel.Logger
{
    public class MessageSink : ILogEventSink
    {
        private readonly ITextFormatter formatter;
        private readonly Action<string> writer;

        public MessageSink(ITextFormatter formatter, Action<string> writer)
        {
            this.formatter = formatter;
            this.writer = writer;
        }

        public void Emit(LogEvent logEvent)
        {
            using StringWriter stringWriter = new StringWriter();
            formatter.Format(logEvent, stringWriter);
            stringWriter.Flush();
            writer(stringWriter.GetStringBuilder().ToString());
        }
    }

    public static class MessageSinkExtensions
    {
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
}
