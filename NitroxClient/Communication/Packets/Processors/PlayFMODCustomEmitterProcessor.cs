using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlayFMODCustomEmitterProcessor : ClientPacketProcessor<PlayFMODCustomEmitter>
    {
        private readonly IPacketSender packetSender;

        public PlayFMODCustomEmitterProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }


        public override void Process(PlayFMODCustomEmitter packet)
        {
            Optional<GameObject> soundSource = NitroxEntity.GetObjectFrom(packet.Id);
            Validate.IsPresent(soundSource);

            FMODEmitterController fmodEmitterController = soundSource.Value.GetComponent<FMODEmitterController>();
            Validate.IsTrue(fmodEmitterController);

            using (packetSender.Suppress<PlayFMODCustomEmitter>())
            {
                if (packet.Play)
                {
                    fmodEmitterController.PlayCustomEmitter(packet.AssetPath);
                }
                else
                {
                    fmodEmitterController.StopCustomEmitter(packet.AssetPath);
                }
            }
        }
    }
}
