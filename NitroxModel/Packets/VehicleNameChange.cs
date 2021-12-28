using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class VehicleNameChange : Packet
    {
        [Index(0)]
        public virtual Optional<NitroxId> ParentId { get; protected set; }
        [Index(1)]
        public virtual NitroxId VehicleId { get; protected set; }
        [Index(2)]
        public virtual string Name { get; protected set; }

        public VehicleNameChange() { }

        public VehicleNameChange(NitroxId parentId, NitroxId vehicleId, string name)
        {
            ParentId = Optional.OfNullable(parentId);
            VehicleId = vehicleId;
            Name = name;
        }
    }
}
