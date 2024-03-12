using System;
using NitroxModel.Platforms.Store.Interfaces;
using static NitroxModel.DisplayStatusCodes;
namespace NitroxModel.Platforms.Store.Exceptions
{
    public class PlatformException : Exception
    {
        public PlatformException(IGamePlatform platform, string message) : base($"{platform.GetType().Name}: {message}")
        {
            DisplayStatusCode(StatusCode.MISSING_FEATURE, true, "Tried to access a feature that does not exist yet");
        }

        public PlatformException(IGamePlatform platform, string message, Exception innerException) : base($"{platform.GetType().Name}: {message}", innerException)
        {
            DisplayStatusCode(StatusCode.MISSING_FEATURE, true, "Tried to access a feature that does not exist yet");
        }
    }
}
