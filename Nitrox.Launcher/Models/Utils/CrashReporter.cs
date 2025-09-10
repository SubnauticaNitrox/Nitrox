using System;
using System.IO;
using System.Linq;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Platforms.OS.Shared;

namespace Nitrox.Launcher.Models.Utils;

internal static class CrashReporter
{
    private const string CRASH_REPORT_FILE_NAME = "Nitrox.Launcher.crash";

    public static void ReportAndExit(Exception ex)
    {
        Log.Error(ex, "!!!Nitrox Launcher Crash!!!");

        try
        {
            string crashPath = NitroxUser.CrashLogsPath;
            if (!string.IsNullOrWhiteSpace(crashPath))
            {
                Directory.CreateDirectory(crashPath);
                File.WriteAllText(GetNewCrashFilePath(crashPath), ex.ToString());
                ProcessEx.StartSelf("--crash-report");
            }
            else
            {
                Log.Error(ex, "Unable to get crash file path for writing crash report.");
            }
        }
        catch (Exception exception)
        {
            Log.Error(exception);
        }
        finally
        {
            Environment.Exit(1);
        }

        static string GetNewCrashFilePath(string crashPath)
        {
            string crashDate = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");
            int fileInDayCount = Directory.EnumerateFiles(crashPath).Count(f => f.Contains(crashDate));
            string crashFileName = Path.GetFileNameWithoutExtension(CRASH_REPORT_FILE_NAME);
            return Path.Combine(crashPath, $"{crashFileName}_{crashDate}_{(fileInDayCount + 1).ToString().PadLeft(3, '0')}{Path.GetExtension(CRASH_REPORT_FILE_NAME)}");
        }
    }

    public static string? GetLastReport()
    {
        string crashLogsPath = NitroxUser.CrashLogsPath;
        if (Directory.Exists(crashLogsPath) && Directory.EnumerateFiles(crashLogsPath).OrderDescending().FirstOrDefault() is { } crashLog)
        {
            return File.ReadAllText(crashLog);
        }
        return null;
    }
}
