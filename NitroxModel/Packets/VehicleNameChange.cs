using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleNameChange : Packet
    {
        public string Guid { get; }
        public string Name { get; }

        public VehicleNameChange(string guid, string name)
        {
            Guid = guid;
            Name = name;
        }

        public override string ToString()
        {
            return "[VehicleNameChange Guid: " + Guid + " Name: " + Name + "]";
        }
    }
}
