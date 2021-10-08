using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PingRenamed : Packet
    {
        public NitroxId Id { get; }
        public string Name { get; }
        public byte[] BeaconGameObjectSerialized { get; }

        public PingRenamed(NitroxId id, string name, byte[] beaconGameObjectSerialized)
        {
            Id = id;
            Name = name;
            BeaconGameObjectSerialized = beaconGameObjectSerialized;
        }
    }
}
