using System;
using System.Linq;

namespace NitroxModel.Extensions;

public static class ExceptionExtensions
{
    public static string GetFirstNonAggregateMessage(this Exception exception) => exception switch
    {
        AggregateException ex => ex.InnerExceptions.FirstOrDefault(e => e is not AggregateException)?.Message ?? ex.Message,
        _ => exception.Message
    };
}
