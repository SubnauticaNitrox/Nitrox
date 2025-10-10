using System;
using System.Collections;

namespace NitroxClient.Extensions;

public static class CoroutineExtensions
{
    public static IEnumerator OnYieldError(this IEnumerator enumerator, Action<Exception> exceptionCallback)
    {
        return enumerator.OnYieldError<Exception>(exceptionCallback);
    }

    public static IEnumerator OnYieldError<T>(this IEnumerator enumerator, Action<T> exceptionCallback = null) where T : Exception
    {
        if (enumerator == null)
        {
            yield break;
        }
        while (true)
        {
            try
            {
                if (!enumerator.MoveNext())
                {
                    yield break;
                }
            }
            catch (T exception)
            {
                exceptionCallback?.Invoke(exception);
                yield break;
            }
            yield return enumerator.Current;
        }
    }
}
