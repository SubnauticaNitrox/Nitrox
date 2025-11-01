using System;
using Nitrox.Model.Platforms.Discovery.Models;
using Nitrox.Model.Platforms.Store.Interfaces;

namespace Nitrox.Model.Platforms.Store.Exceptions;

/// <summary>
///     Exception to be thrown when an issue with a game store occurs.
/// </summary>
public class GamePlatformException : Exception
{
    public GamePlatformException(IGamePlatform platform, string message) : base($"{platform.GetType().Name}: {message}")
    {
    }

    public GamePlatformException(IGamePlatform platform, string message, Exception innerException) : base($"{platform.GetType().Name}: {message}", innerException)
    {
    }

    public GamePlatformException(GameLibraries library, string message) : base($"{Enum.GetName(typeof(GameLibraries), library)}: {message}")
    {
    }

    public GamePlatformException(GameLibraries library, string message, Exception innerException) : base($"{Enum.GetName(typeof(GameLibraries), library)}: {message}", innerException)
    {
    }
}
