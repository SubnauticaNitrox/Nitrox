using System;
using static NitroxModel.DisplayStatusCodes;
namespace NitroxClient.Communication.Exceptions
{
    public class ClientConnectionFailedException : Exception
    {
        public ClientConnectionFailedException(string message) : base(message)
        {
            DisplayStatusCode(StatusCode.connectionFailClient, false);
        }

        public ClientConnectionFailedException(string message, Exception innerException) : base(message, innerException)
        {
            DisplayStatusCode(StatusCode.connectionFailClient, false);
        }
    }
}
