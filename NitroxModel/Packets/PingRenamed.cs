using System;
using NitroxModel.DataStructures;
using ProtoBufNet;

namespace NitroxModel.Packets
{
    [Serializable]
    [ProtoContract]
    public class PingRenamed : Packet
    {
        [ProtoMember(1)]
        public NitroxId Id { get; }

        [ProtoMember(2)]
        public string Name { get; }
        
        [ProtoMember(3)]
        public byte[] BeaconGameObjectSerialized { get; }

        /// <summary>
        ///     Constructor for serialization
        /// </summary>
        public PingRenamed()
        {
        }

        public PingRenamed(NitroxId id, string name, byte[] beaconGameObjectSerialized)
        {
            Id = id;
            Name = name;
            BeaconGameObjectSerialized = beaconGameObjectSerialized;
        }

        public override string ToString()
        {
            return $"[{nameof(PingRenamed)} {{{nameof(Id)}: {Id}}}, {{{nameof(Name)}: {Name}}}, {{{BeaconGameObjectSerialized}.Length: {BeaconGameObjectSerialized?.Length}}}]";
        }
    }
}
