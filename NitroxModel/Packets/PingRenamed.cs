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

        /// <summary>
        ///     Constructor for serialization
        /// </summary>
        public PingRenamed()
        {
        }

        public PingRenamed(NitroxId id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return $"[{nameof(PingRenamed)} {{{nameof(Id)}: {Id}}}, {{{nameof(Name)}: {Name}}}]";
        }
    }
}
