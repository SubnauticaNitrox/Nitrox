using System;
using System.IO;
using Avalonia.Controls;
using Nitrox.Model.Platforms.OS.Shared;

namespace Nitrox.Launcher;

internal static class GlobalStatic
{
    public static bool IsDesignMode => Design.IsDesignMode;

    /// <inheritdoc cref="ProcessEx.OpenUri" />
    public static void OpenUri(string url) => ProcessEx.OpenUri(url);

    /// <inheritdoc cref="ProcessEx.OpenDirectory" />
    public static bool OpenDirectory(string? directory) => ProcessEx.OpenDirectory(directory);

    public static bool TryDeleteFile(string filePath)
    {
        try
        {
            File.Delete(filePath);
            return true;
        }
        catch (Exception ex) when (ex is FileNotFoundException or UnauthorizedAccessException)
        {
            // ignored
        }
        return false;
    }

    /// <inheritdoc cref="Directory.Delete(string, bool)" />
    public static bool TryDeleteDirectory(string directory, bool recursive = false)
    {
        try
        {
            Directory.Delete(directory, recursive);
            return true;
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException)
        {
            // ignored
        }
        return false;
    }

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
