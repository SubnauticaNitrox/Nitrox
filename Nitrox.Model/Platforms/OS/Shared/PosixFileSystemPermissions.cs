using System;
using System.Diagnostics;

namespace Nitrox.Model.Platforms.OS.Shared;

/// <summary>
///     POSIX chmod-based permission updates shared by macOS and Linux <see cref="FileSystem" /> implementations.
/// </summary>
internal static class PosixFileSystemPermissions
{
    public static bool SetFullAccessToCurrentUser(string directory)
    {
        try
        {
            string escapedPath = directory.Replace("\"", "\\\"", StringComparison.Ordinal);
            using Process process = Process.Start(new ProcessStartInfo
            {
                FileName = "chmod",
                Arguments = $"-R u+rwX \"{escapedPath}\"",
                RedirectStandardError = true,
                UseShellExecute = false,
            })!;
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
