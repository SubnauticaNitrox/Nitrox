using System;
using System.IO;
using System.Threading;
using Avalonia.Controls;
using Nitrox.Model.Platforms.OS.Shared;

namespace Nitrox.Launcher;

internal static class GlobalStatic
{
    private const string SERVER_FILE_NAME_NO_EXT = "Nitrox.Server.Subnautica";
    private static long isSteamOs = -1;

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

    public static string GetServerFileName()
    {
        if (OperatingSystem.IsWindows())
        {
            return $"{SERVER_FILE_NAME_NO_EXT}.exe";
        }

        return SERVER_FILE_NAME_NO_EXT;
    }

    public static string GetServerProcessName()
    {
        // On Steam Deck, server process name is "dotnet" as it's started through command line.
        if (IsSteamOs())
        {
            return "dotnet";
        }

        return GetServerFileName();
    }

    public static bool IsSteamOs()
    {
        if (!OperatingSystem.IsLinux())
        {
            return false;
        }
        switch (Interlocked.Read(ref isSteamOs))
        {
            case 1:
                return true;
            case 0:
                return false;
        }

        try
        {
            string osReleaseInfo = File.ReadAllText("/etc/os-release");
            bool result = osReleaseInfo.Contains(@"NAME=""SteamOS""", StringComparison.Ordinal);
            Interlocked.Exchange(ref isSteamOs, result ? 1 : 0);
            return result;
        }
        catch (IOException)
        {
            return false;
        }
    }
}
