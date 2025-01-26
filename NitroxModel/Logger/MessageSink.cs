using System;
using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

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
            using StringWriter stringWriter = new();
            formatter.Format(logEvent, stringWriter);
            stringWriter.Flush();
            writer(stringWriter.GetStringBuilder().ToString());
        }
    }
}
