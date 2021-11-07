using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CameraMovementProcessor : ClientPacketProcessor<CameraMovement>
    {
        public CameraMovementProcessor()
        {
            // TornacTODO: Include a "movement smoother"
        }

        public override void Process(CameraMovement cameraMovement)
        {
            if (NitroxEntity.TryGetObjectFrom(cameraMovement.CameraMovementData.Id, out GameObject cameraObject))
            {
                cameraObject.transform.position = cameraMovement.Position.ToUnity();
                cameraObject.transform.rotation = cameraMovement.BodyRotation.ToUnity();
            }
        }
    }
}
