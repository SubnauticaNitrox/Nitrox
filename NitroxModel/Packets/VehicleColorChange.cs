using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class VehicleColorChange : Packet
    {
        [Index(0)]
        public virtual Optional<NitroxId> ParentId { get; protected set; }
        [Index(1)]
        public virtual NitroxId VehicleId { get; protected set; }
        [Index(2)]
        public virtual int Index { get; protected set; }
        [Index(3)]
        public virtual NitroxVector3 HSB { get; protected set; }
        [Index(4)]
        public virtual NitroxColor Color { get; protected set; }

        private VehicleColorChange() { }

        public VehicleColorChange(int index, NitroxId parentId, NitroxId vehicleId, NitroxVector3 hsb, NitroxColor color)
        {
            ParentId = Optional.OfNullable(parentId);
            VehicleId = vehicleId;
            Index = index;
            HSB = hsb;
            Color = color;
        }
    }
}
