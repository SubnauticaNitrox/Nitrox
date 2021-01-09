using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlayFMODCustomLoopingEmitterProcessor : ClientPacketProcessor<PlayFMODCustomLoopingEmitter>
    {
        public override void Process(PlayFMODCustomLoopingEmitter packet)
        {
            Optional<GameObject> soundSource = NitroxEntity.GetObjectFrom(packet.Id);
            Validate.IsPresent(soundSource);

            FMODEmitterController fmodEmitterController = soundSource.Value.GetComponent<FMODEmitterController>();
            Validate.IsTrue(fmodEmitterController);

            fmodEmitterController.PlayCustomLoopingEmitter(packet.AssetPath);
        }
    }
}
