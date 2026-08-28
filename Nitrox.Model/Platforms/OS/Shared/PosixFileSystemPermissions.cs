using System.Diagnostics;
using System.IO;

namespace Nitrox.Model.Platforms.OS.Shared;

/// <summary>
///     POSIX chmod-based permission updates shared by macOS and Linux <see cref="FileSystem" /> implementations.
/// </summary>
internal static class PosixFileSystemPermissions
{
    public static bool SetFullAccessToCurrentUser(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            return false;
        }

        // Don't try to execute anything on root folder ("/")
        if (IsRootDirectory(directory))
        {
            Log.Error($"Tried to set full access to current user on root directory '{directory}'");
            return false;
        }

        try
        {
            string path = Path.GetFullPath(directory);
            string escapedPath = path.Replace("\"", "\\\"");

            using Process? process = Process.Start(new ProcessStartInfo
            {
                FileName = "chmod",
                Arguments = $"-R u+rwX \"{escapedPath}\"",
                RedirectStandardError = true,
                UseShellExecute = false,
            });

            if (process is null)
            {
                return false;
            }

            if (!process.HasExited)
            {
                process.WaitForExit();
            }

            return process.ExitCode == 0;
        }
        catch
        {
            Log.Warn($"Failed to set full access of '{directory}' to current user");
            return false;
        }
    }

    public static bool IsRootDirectory(string path)
    {
        string? root = Path.GetPathRoot(path);
        if (string.IsNullOrWhiteSpace(root))
        {
            return false;
        }

        return string.Equals(
            path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            System.StringComparison.Ordinal
            );
    }
}
