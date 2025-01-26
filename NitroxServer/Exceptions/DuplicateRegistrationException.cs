using System;

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
    }
}
