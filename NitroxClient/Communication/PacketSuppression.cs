using System;
using System.Collections.Generic;

namespace NitroxClient.Communication
{
    public class PacketSuppression<T> : IDisposable
    {
        private readonly HashSet<Type> suppressedPacketsTypes;

        public PacketSuppression(HashSet<Type> suppressedPacketsTypes)
        {
            this.suppressedPacketsTypes = suppressedPacketsTypes;
            suppressedPacketsTypes.Add(typeof(T));
        }

        public void Dispose()
        {
            suppressedPacketsTypes.Remove(typeof(T));
        }
    }
}
