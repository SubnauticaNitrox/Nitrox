using FMOD.Studio;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlayFMOD_CustomEmitterProcessor : ClientPacketProcessor<PlayFMOD_CustomEmitter>
    {
        private readonly IPacketSender packetSender;

        public PlayFMOD_CustomEmitterProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }


        public override void Process(PlayFMOD_CustomEmitter packet)
        {
            Optional<GameObject> soundSource = NitroxEntity.GetObjectFrom(packet.Id);
            if (!soundSource.HasValue)
            {
                return;
            }

            FMOD_CustomEmitter fmodCustomEmitter = soundSource.Value.GetComponents<FMOD_CustomEmitter>()[packet.ComponentId];
            if (!fmodCustomEmitter)
            {
                return;
            }

            using (packetSender.Suppress<PlayFMOD_CustomEmitter>())
            {
                if (packet.Play)
                {
                    fmodCustomEmitter.Play();
                }
                else
                {
                    fmodCustomEmitter.Stop();
                }
            }
        }
    }
}
