using System;
using System.Collections.Generic;
using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace NitroxModel.Logger;

public class ConditionalValveSink : ILogEventSink
{
    private readonly Func<LogEvent, bool> thresholdPredicate;
    private readonly ILogEventSink wrappedSink;
    private readonly Queue<LogEvent> queue = new();

    public ConditionalValveSink(Func<LogEvent, bool> thresholdPredicate, ILogEventSink wrappedSink)
    {
        this.thresholdPredicate = thresholdPredicate;
        this.wrappedSink = wrappedSink;
    }

    public void Emit(LogEvent logEvent)
    {
        if (thresholdPredicate?.Invoke(logEvent) == true)
        {
            while (queue.Count > 0 && queue.Dequeue() is { } dequeuedEvent)
            {
                foreach (KeyValuePair<string,LogEventPropertyValue> pair in logEvent.Properties)
                {
                    dequeuedEvent.AddOrUpdateProperty(new LogEventProperty(pair.Key, pair.Value));
                }
                wrappedSink.Emit(dequeuedEvent);
            }

            wrappedSink.Emit(logEvent);
        }
        else
        {
            queue.Enqueue(logEvent);
        }
    }
}