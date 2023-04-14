using System;
using System.Collections;

namespace NitroxModel.Helper;

public static class IEnumeratorHelper
{
    /// <remarks>https://stackoverflow.com/questions/5067188/yield-return-with-try-catch-how-can-i-solve-it</remarks>>
    public static IEnumerator YieldWithTryCatch(IEnumerator enumerator, string errorMessage)
    {
        while (true)
        {
            object ret = null;
            try
            {
                if (!enumerator.MoveNext())
                {
                    break;
                }
                ret = enumerator.Current;
            }
            catch (Exception ex)
            {
                Log.Error(ex, errorMessage);
                break;
            }

            yield return ret;
        }
    }
}
