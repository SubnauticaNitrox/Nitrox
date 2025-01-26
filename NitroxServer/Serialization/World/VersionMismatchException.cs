using System;
using System.Runtime.Serialization;

namespace NitroxServer.Serialization.World
{
    [Serializable]
    public class VersionMismatchException : Exception
    {
        public VersionMismatchException() { }

        public VersionMismatchException(string message) : base(message) { }

        public VersionMismatchException(string message, Exception inner) : base(message, inner) { }

        public override string ToString()
        {
            return base.Message;
        }
    }
}
