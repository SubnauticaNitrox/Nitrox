using System;
using System.Threading.Tasks;

namespace NitroxModel.Extensions;

public static class TaskExtensions
{
    /// <summary>
    ///     Calls an action if an error happens.
    /// </summary>
    /// <remarks>
    ///     Use this for fire-and-forget tasks so that errors aren't hidden when they happen.
    /// </remarks>
    public static Task ContinueWithHandleError(this Task task, Action<Exception> onError) =>
        task.ContinueWith(t =>
        {
            if (t is not { IsFaulted: true, Exception: { } ex })
            {
                return;
            }

            onError(ex);
        });

    /// <summary>
    ///    Logs any exception/error of the task.
    /// </summary>
    /// <remarks>
    ///    <inheritdoc cref="ContinueWithHandleError(System.Threading.Tasks.Task,System.Action{System.Exception})"/>
    /// </remarks>
    public static Task ContinueWithHandleError(this Task task, bool alsoLogIngame = false) => task.ContinueWithHandleError(ex =>
    {
        Log.Error(ex);
        if (alsoLogIngame)
        {
            Log.InGame(ex.Message);
        }
    });
}
