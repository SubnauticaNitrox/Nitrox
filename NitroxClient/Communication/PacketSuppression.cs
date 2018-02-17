using System;
using System.Collections.Generic;

namespace NitroxClient.Communication
{
    public class PacketSuppressor<T> : IDisposable
    {
        private readonly HashSet<Type> suppressedPacketTypes;

        public PacketSuppressor(HashSet<Type> suppressedPacketTypes)
        {
            this.suppressedPacketTypes = suppressedPacketTypes;
            suppressedPacketTypes.Add(typeof(T));
        }

        public void Dispose()
        {
            suppressedPacketTypes.Remove(typeof(T));
        }
    }
}
