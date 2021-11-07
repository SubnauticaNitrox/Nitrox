using System;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Networking;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CameraMovement : Movement
    {
        public CameraMovementData CameraMovementData { get; }

        public CameraMovement(ushort playerId, CameraMovementData cameraMovementData) : base(playerId, cameraMovementData.Position, NitroxVector3.Zero, cameraMovementData.Rotation, NitroxQuaternion.Identity)
        {
            CameraMovementData = cameraMovementData;
            DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.UNRELIABLE_SEQUENCED;
            UdpChannel = UdpChannelId.CAMERA_MOVEMENT;
        }
    }
}
