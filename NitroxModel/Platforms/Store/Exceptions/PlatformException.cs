using System;
using NitroxModel.Platforms.Store.Interfaces;
using static NitroxServer.Server;
using static NitroxModel.DisplayStatusCodes;
namespace NitroxModel.Platforms.Store.Exceptions
{
    public class PlatformException : Exception
    {
        public PlatformException(IGamePlatform platform, string message) : base($"{platform.GetType().Name}: {message}")
        {
            DisplayStatusCode(StatusCode.missingFeature, true);
        }

        public PlatformException(IGamePlatform platform, string message, Exception innerException) : base($"{platform.GetType().Name}: {message}", innerException)
        {
            DisplayStatusCode(StatusCode.missingFeature, true);
        }
    }
}
