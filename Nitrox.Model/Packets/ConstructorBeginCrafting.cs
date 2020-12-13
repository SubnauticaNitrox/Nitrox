using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Model.Packets
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

        public override string ToString()
        {
            return $"[ConstructorBeginCrafting - VehicleModel: {VehicleModel}, ConstructorId: {ConstructorId}, Duration: {Duration}";
        }
    }
}
