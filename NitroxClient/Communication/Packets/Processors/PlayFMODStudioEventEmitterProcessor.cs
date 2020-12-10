using FMOD.Studio;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlayFMODStudioEventEmitterProcessor : ClientPacketProcessor<PlayFMODStudioEmitter>
    {
        private readonly IPacketSender packetSender;

        public PlayFMODStudioEventEmitterProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }


        public override void Process(PlayFMODStudioEmitter packet)
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

            using (packetSender.Suppress<PlayFMODStudioEmitter>())
            {
                if (packet.Play)
                {
                    fmodEmitterController.PlayStudioEmitter(packet.AssetPath);
                }
                else
                {
                    fmodEmitterController.StopStudioEmitter(packet.AssetPath, packet.AllowFadeout);
                }
            }
        }
    }
}
