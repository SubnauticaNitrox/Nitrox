using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleColorChange : Packet
    {
        public Optional<NitroxId> ParentId { get; }
        public NitroxId VehicleId { get; }
        public int Index { get; }
        public NitroxVector3 HSB { get; }
        public NitroxColor Color { get; }

        public VehicleColorChange(NitroxId parentId, NitroxId vehicleId, int index, NitroxVector3 hsb, NitroxColor color)
        {
            ParentId = Optional.OfNullable(parentId);
            VehicleId = vehicleId;
            Index = index;
            HSB = hsb;
            Color = color;
        }

        /// <remarks>Used for deserialization</remarks>
        public VehicleColorChange(Optional<NitroxId> parentId, NitroxId vehicleId, int index, NitroxVector3 hsb, NitroxColor color)
        {
            ParentId = parentId;
            VehicleId = vehicleId;
            Index = index;
            HSB = hsb;
            Color = color;
        }
    }
}
