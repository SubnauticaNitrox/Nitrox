using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxServer.Serialization.World
{
    [Serializable]
    public class WorldVersionMismatchException : Exception
    {
        public WorldVersionMismatchException() { }
        public WorldVersionMismatchException(string message) : base(message) { }
        public WorldVersionMismatchException(string message, Exception inner) : base(message, inner) { }
        protected WorldVersionMismatchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public override string ToString()
        {
            return base.Message;
        }
    }
}
