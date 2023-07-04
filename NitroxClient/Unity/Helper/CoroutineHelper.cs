using System;
using System.Collections;

namespace NitroxClient.Unity.Helper;

public static class CoroutineHelper
{
    public static IEnumerator SafelyYieldEnumerator(IEnumerator enumerator, Action<Exception> exceptionCallback)
    {
        yield return SafelyYieldEnumerator<Exception>(enumerator, exceptionCallback);
    }

    public static IEnumerator SafelyYieldEnumerator<T>(IEnumerator enumerator, Action<T> exceptionCallback = null) where T : Exception
    {
        if (enumerator == null)
        {
            yield break;
        }
        for (; ; )
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
