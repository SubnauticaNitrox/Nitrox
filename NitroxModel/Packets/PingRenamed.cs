using NitroxModel.DataStructures;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class PingRenamed : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual string Name { get; protected set; }
        [Index(2)]
        public virtual byte[] BeaconGameObjectSerialized { get; protected set; } // TODO: Change this to send only the important info

        private PingRenamed() { }

        public PingRenamed(NitroxId id, string name, byte[] beaconGameObjectSerialized)
        {
            Id = id;
            Name = name;
            BeaconGameObjectSerialized = beaconGameObjectSerialized;
        }
    }
}
