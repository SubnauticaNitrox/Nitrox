using System;
using System.Collections.Generic;
using Avalonia.Controls;

namespace Nitrox.Launcher.Models.Extensions;

/// <summary>
///     Avalonia doesn't provide a public API to close the window non-programmatically so this is a hack to support it.
/// </summary>
public static class CloseByUserExtensions
{
    private static readonly Dictionary<Window, bool> isClosingProgrammatically = [];

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

        isClosingProgrammatically[window] = true;
        window.Close();
        return;

        static void WindowOnClosed(object sender, EventArgs e)
        {
            if (sender is not Window window)
            {
                return;
            }
            window.Closed -= WindowOnClosed;
            isClosingProgrammatically.Remove(window);
        }
    }

    public static bool IsClosingByUser(this WindowClosingEventArgs args, Window closingWindow)
    {
        if (!args.IsProgrammatic)
        {
            return true;
        }
        if (closingWindow is not null && isClosingProgrammatically.TryGetValue(closingWindow, out bool isByUser))
        {
            return isByUser;
        }
        return false;
    }
}
