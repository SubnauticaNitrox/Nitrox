using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets
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
            return "[VehicleNameChange Id: " + Id + " Name: " + Name + "]";
        }
    }
}
