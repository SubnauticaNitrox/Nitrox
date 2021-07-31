using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ConstructorBeginCrafting : Packet
    {
        public VehicleModel VehicleModel { get; }
        public NitroxId ConstructorId { get; }
        public float Duration { get; }

        public ConstructorBeginCrafting(VehicleModel vehicleModel, NitroxId constructorId, float duration)
        {
            ConstructorId = constructorId;
            VehicleModel = vehicleModel;
            Duration = duration;
        }
    }
}
