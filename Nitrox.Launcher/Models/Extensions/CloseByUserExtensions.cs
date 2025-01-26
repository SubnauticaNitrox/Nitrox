using System;
using System.Collections.Generic;
using Avalonia.Controls;

namespace Nitrox.Launcher.Models.Extensions;

/// <summary>
///     Avalonia doesn't provide a public API to close the window non-programmatically so this is a hack to support it.
/// </summary>
public static class CloseByUserExtensions
{
    private static readonly Dictionary<Window, bool> isClosingByUser = [];

    /// <summary>
    ///     Closes the window non-programmatically (by user).
    /// </summary>
    public static void CloseByUser(this Window window)
    {
        if (window == null)
        {
            return;
        }
        window.Closed += WindowOnClosed;
        isClosingByUser[window] = true;
        window.Close();

        static void WindowOnClosed(object sender, EventArgs e)
        {
            if (sender is not Window window)
            {
                return;
            }
            window.Closed -= WindowOnClosed;
            isClosingByUser.Remove(window);
        }
    }

    /// <summary>
    ///     Closes the window programmatically.
    /// </summary>
    public static void CloseByCode(this Window window)
    {
        if (window == null)
        {
            return;
        }
        isClosingByUser[window] = false;
        window.Close();
    }

    public static bool IsClosingByUser(this Window closingWindow, WindowClosingEventArgs closingArgs = null)
    {
        if (closingWindow is not null && isClosingByUser.TryGetValue(closingWindow, out bool isByUser))
        {
            return isByUser;
        }
        if (closingArgs is { IsProgrammatic: false })
        {
            return true;
        }
        return false;
    }
}
