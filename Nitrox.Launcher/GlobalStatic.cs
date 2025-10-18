using System;
using Avalonia.Controls;
using Nitrox.Model.Platforms.OS.Shared;

namespace Nitrox.Launcher;

internal static class GlobalStatic
{
    public static bool IsDesignMode => Design.IsDesignMode;

    /// <inheritdoc cref="ProcessEx.OpenUri" />
    public static void OpenUri(string url) => ProcessEx.OpenUri(url);

    public static string GetServerExeName()
    {
        string serverExeName = "Nitrox.Server.Subnautica.exe";
        if (!OperatingSystem.IsWindows())
        {
            serverExeName = "Nitrox.Server.Subnautica";
        }
        return serverExeName;
    }
}
