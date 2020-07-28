using System;
using System.Runtime.Serialization;

namespace NitroxClient.Communication.Exceptions
{
    [Serializable]
    public class ClientConnectionFailedException : Exception
    {
        public ClientConnectionFailedException(string message) : base(message)
        {
        }

        public ClientConnectionFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ClientConnectionFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
