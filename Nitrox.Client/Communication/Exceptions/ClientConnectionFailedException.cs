using System;

namespace Nitrox.Client.Communication.Exceptions
{
    public class ClientConnectionFailedException : Exception
    {
        public ClientConnectionFailedException(string message) : base(message)
        {
        }

        public ClientConnectionFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
