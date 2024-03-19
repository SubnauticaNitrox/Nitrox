using System;
using static NitroxModel.DisplayStatusCodes;
namespace NitroxClient.Communication.Exceptions
{
    public class ClientConnectionFailedException : Exception
    {
        public ClientConnectionFailedException(string message) : base(message)
        {
            DisplayStatusCode(StatusCode.CONNECTION_FAIL_CLIENT, false, "Client failed to connect to the server and has been disconnected" + message);
        }

        public ClientConnectionFailedException(string message, Exception innerException) : base(message, innerException)
        {
            DisplayStatusCode(StatusCode.CONNECTION_FAIL_CLIENT, false, "Client failed to connect to the server and has been disconnected" + message);
        }
    }
}
