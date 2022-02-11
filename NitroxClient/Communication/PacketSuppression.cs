using System;
using System.Collections.Generic;
using NitroxModel.Packets;

namespace NitroxClient.Communication
{
    public class PacketSuppressor<T> : IDisposable
    {
        private readonly HashSet<Type> suppressedPacketTypes;
        private readonly bool suppressed;

        public PacketSuppressor(HashSet<Type> suppressedPacketTypes)
        {
            this.suppressedPacketTypes = suppressedPacketTypes;
            suppressed = suppressedPacketTypes.Add(typeof(T));
        }

        public void Dispose()
        {
            if (suppressed)
            {
                suppressedPacketTypes.Remove(typeof(T));
            }
        }
    }

    public class SoundPacketSuppressor : IDisposable
    {
        private readonly HashSet<Type> suppressedPacketTypes;
        private readonly HashSet<Type> soundPackets = new() { typeof(PlayFMODAsset), typeof(PlayFMODCustomEmitter), typeof(PlayFMODCustomLoopingEmitter), typeof(PlayFMODEventInstance), typeof(PlayFMODStudioEmitter) };

        public SoundPacketSuppressor(HashSet<Type> suppressedPacketTypes)
        {
            this.suppressedPacketTypes = suppressedPacketTypes;
            suppressedPacketTypes.AddRange(soundPackets);
        }

        public void Dispose()
        {
            suppressedPacketTypes.RemoveRange(soundPackets);
        }
    }

    public class PacketUnsuppressor<T> : IDisposable
    {
        private readonly HashSet<Type> suppressedPacketTypes;
        private readonly bool unsuppressed;

        public PacketUnsuppressor(HashSet<Type> suppressedPacketTypes)
        {
            this.suppressedPacketTypes = suppressedPacketTypes;
            unsuppressed = suppressedPacketTypes.Remove(typeof(T));
        }

        public void Dispose()
        {
            if (unsuppressed)
            {
                suppressedPacketTypes.Add(typeof(T));
            }
        }
    }
}
