using System;
using System.Runtime.Serialization;

namespace NitroxServer.Exceptions
{
    [Serializable]
    internal class DuplicateRegistrationException : Exception
    {
        public DuplicateRegistrationException()
        {
        }

        public DuplicateRegistrationException(string message) : base(message)
        {
        }

        public DuplicateRegistrationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DuplicateRegistrationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
