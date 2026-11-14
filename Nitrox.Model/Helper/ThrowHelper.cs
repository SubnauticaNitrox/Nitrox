using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Nitrox.Model.Helper;

/// <summary>
///     Helper for throwing exceptions. Using this helper is preferred over manually throwing for performance reasons. See:
///     https://learn.microsoft.com/en-us/dotnet/communitytoolkit/diagnostics/throwhelper
/// </summary>
[DebuggerStepThrough]
#if NET
[StackTraceHidden]
#endif
internal static class ThrowHelper
{
    [DoesNotReturn]
    public static void ThrowAlreadyDisposed(string objectName) => throw new ObjectDisposedException(objectName);

    [DoesNotReturn]
    public static void ThrowInvalidOperation() => throw new InvalidOperationException();
}
