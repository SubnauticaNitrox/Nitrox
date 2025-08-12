extern alias JB;
using System;
using System.Runtime.CompilerServices;
using JB::JetBrains.Annotations;
using NitroxModel.Discovery.Models;
using NitroxModel.Helper;

namespace NitroxModel.Discovery.InstallationFinders.Core;

public sealed record GameFinderResult
{
    public string ErrorMessage { get; init; }
    public GameLibraries Origin { get; init; }
    public string Path { get; init; }

    /// <summary>
    ///     Gets the name of type that made the result.
    /// </summary>
    public string FinderName { get; init; } = "";

    public bool IsOk => string.IsNullOrWhiteSpace(ErrorMessage) && !string.IsNullOrWhiteSpace(Path);

    private GameFinderResult()
    {
    }

    public static GameFinderResult Error(string message, [CallerFilePath] string callerCodeFile = "")
    {
        return new GameFinderResult
        {
            FinderName = callerCodeFile[(callerCodeFile.LastIndexOf("\\", StringComparison.Ordinal) + 1)..^3],
            ErrorMessage = message
        };
    }

    /// <summary>
    ///     Returned when game libraries were found but the game appears to not be installed.
    /// </summary>
    public static GameFinderResult NotFound([CallerFilePath] string callerCodeFile = "")
    {
        return new GameFinderResult
        {
            FinderName = callerCodeFile[(callerCodeFile.LastIndexOf("\\", StringComparison.Ordinal) + 1)..^3]
        };
    }

    public static GameFinderResult Ok([NotNull] string path, [CallerFilePath] string callerCodeFile = "")
    {
        Validate.NotNull(path);
        return new GameFinderResult
        {
            FinderName = callerCodeFile[(callerCodeFile.LastIndexOf("\\", StringComparison.Ordinal) + 1)..^3],
            Path = path
        };
    }
}
