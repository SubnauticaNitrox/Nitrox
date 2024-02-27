using System;
using System.Runtime.CompilerServices;
using NitroxModel.Discovery.Models;

namespace NitroxModel.Discovery.InstallationFinders.Core;

public sealed class GameFinderResult
{
    public GameInstallation Installation { get; init; }
    public string ErrorMessage { get; init; }

    /// <summary>
    ///     Gets the name of type that made the result.
    /// </summary>
    public string FinderName { get; init; } = "";

    public bool IsOk => string.IsNullOrWhiteSpace(ErrorMessage) && Installation != null;

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

    public static GameFinderResult Ok(GameInstallation installation, [CallerFilePath] string callerCodeFile = "")
    {
        return new GameFinderResult
        {
            FinderName = callerCodeFile[(callerCodeFile.LastIndexOf("\\", StringComparison.Ordinal) + 1)..^3],
            Installation = installation
        };
    }
}
