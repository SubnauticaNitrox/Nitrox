using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlayFMODCustomLoopingEmitterProcessor : ClientPacketProcessor<PlayFMODCustomLoopingEmitter>
    {
        public override void Process(PlayFMODCustomLoopingEmitter packet)
        {
            Optional<GameObject> soundSource = NitroxEntity.GetObjectFrom(packet.Id);
            if (!soundSource.HasValue)
            {
                return;
            }

            FMODEmitterController fmodEmitterController = soundSource.Value.GetComponent<FMODEmitterController>();
            if (!fmodEmitterController)
            {
                return;
            }

            fmodEmitterController.PlayCustomLoopingEmitter(packet.AssetPath);
        }
    }
}
