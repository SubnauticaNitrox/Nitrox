using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleNameChange : Packet
    {
        public Optional<NitroxId> ParentId { get; }
        public NitroxId VehicleId { get; }
        public string Name { get; }

        public VehicleNameChange(NitroxId parentId, NitroxId vehicleId, string name)
        {
            ParentId = Optional.OfNullable(parentId);
            VehicleId = vehicleId;
            Name = name;
        }
    }
}
