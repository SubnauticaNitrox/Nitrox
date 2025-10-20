﻿using System;
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
}
