using System;
using System.Diagnostics;
using System.Reflection;

namespace NitroxModel.Helper;

/// <summary>
///     Environment helper for getting meta data about where and how Nitrox is running.
/// </summary>
public static class NitroxEnvironment
{
    private static bool hasSet;
    public static string ReleasePhase => IsReleaseMode ? "Alpha" : "InDev";
    public static Version Version => Assembly.GetExecutingAssembly().GetName().Version;

    public static string VersionInfo
    {
        get
        {
            if (IsReleaseMode)
            {
                return $"{ReleasePhase} V{Version} {GitShortHash}";
            }
            return $"{ReleasePhase} {GitShortHash}";
        }
    }

    public static DateTimeOffset BuildDate
    {
        get
        {
            string buildDateText = Assembly.GetExecutingAssembly().GetMetaData("BuildDate");
            return DateTimeOffset.TryParse(buildDateText, out DateTimeOffset result) ? result : default;
        }
    }

    public static string GitShortHash
    {
        get
        {
            if (Assembly.GetExecutingAssembly().GetMetaData("GitShortHash") is { Length: > 0 } shortHash)
            {
                return shortHash;
            }

            string gitHash = GitHash;
            if (gitHash is { Length: > 0 })
            {
                gitHash = gitHash.Substring(0, Math.Min(10, gitHash.Length));
            }
            return gitHash;
        }
    }

    public static string GitHash => Assembly.GetExecutingAssembly().GetMetaData("GitHash") ?? "";

    public static Types Type { get; private set; } = Types.NORMAL;
    public static bool IsTesting => Type == Types.TESTING;
    public static bool IsNormal => Type == Types.NORMAL;

    public static int CurrentProcessId
    {
        get
        {
            using Process process = Process.GetCurrentProcess();
            return process.Id;
        }
    }

    public static bool IsReleaseMode
    {
        get
        {
#if RELEASE
                return true;
#else
            return false;
#endif
        }
    }

    public static string AppName => (Assembly.GetEntryAssembly()?.GetName().Name ?? Assembly.GetCallingAssembly().GetName().Name).Replace(".", " ");

    public static void Set(Types value)
    {
        if (hasSet)
        {
            throw new Exception("Environment type can only be set once");
        }

        Type = value;
        hasSet = true;
    }

    public enum Types
    {
        NORMAL,
        TESTING
    }
}
