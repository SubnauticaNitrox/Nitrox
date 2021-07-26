using System;

namespace NitroxServer.Exceptions
{
    public class DuplicateRegistrationException : Exception
    {
        public DuplicateRegistrationException()
        {
        }

        public DuplicateRegistrationException(string message) : base(message)
        {
        }
    }
}
