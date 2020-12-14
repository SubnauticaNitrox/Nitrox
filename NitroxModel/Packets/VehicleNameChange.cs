using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleNameChange : Packet
    {
        public NitroxId Id { get; }
        public string Name { get; }

        public VehicleNameChange(NitroxId id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return $"[VehicleNameChange - Id: {Id}, Name: {Name}]";
        }
    }
}
