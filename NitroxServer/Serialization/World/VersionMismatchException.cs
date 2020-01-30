using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxServer.Serialization.World
{
    [Serializable]
    public class VersionMismatchException : Exception
    {
        public VersionMismatchException() { }
        public VersionMismatchException(string message) : base(message) { }
        public VersionMismatchException(string message, Exception inner) : base(message, inner) { }
        protected VersionMismatchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public override string ToString()
        {
            return base.Message;
        }
    }
}
