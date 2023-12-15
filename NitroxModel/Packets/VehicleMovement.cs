using System;
using BinaryPack.Attributes;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Networking;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleMovement : Movement
    {
        public override NitroxId Id { get; }
        public VehicleMovementData VehicleMovementData { get; }

        [IgnoredMember]
        public override NitroxVector3 Position => VehicleMovementData.Position;
        [IgnoredMember]
        public override NitroxQuaternion Rotation => VehicleMovementData.Rotation;

        /// <param name="id">Player Id</param>
        public VehicleMovement(NitroxId id, VehicleMovementData vehicleMovementData)
        {
            Id = id;
            VehicleMovementData = vehicleMovementData;
            DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.UNRELIABLE_SEQUENCED;
            UdpChannel = UdpChannelId.VEHICLE_MOVEMENT;
        }
    }
}
