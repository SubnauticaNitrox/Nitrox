using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class ConstructorBeginCrafting : Packet
    {
        [Index(0)]
        public virtual VehicleModel VehicleModel { get; protected set; }
        [Index(1)]
        public virtual NitroxId ConstructorId { get; protected set; }
        [Index(2)]
        public virtual float Duration { get; protected set; }

        public ConstructorBeginCrafting() { }

        public ConstructorBeginCrafting(VehicleModel vehicleModel, NitroxId constructorId, float duration)
        {
            ConstructorId = constructorId;
            VehicleModel = vehicleModel;
            Duration = duration;
        }
    }
}
