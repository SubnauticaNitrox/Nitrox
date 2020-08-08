using System;
using System.Runtime.Serialization;

namespace NitroxLauncher.Exceptions
{
    [Serializable]
    public class MultipleInstancesException : Exception
    {
        public MultipleInstancesException(string message) : base(message)
        {
        }

        protected MultipleInstancesException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
