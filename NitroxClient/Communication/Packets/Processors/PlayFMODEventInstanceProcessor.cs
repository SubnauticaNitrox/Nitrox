using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayFMODEventInstanceProcessor : ClientPacketProcessor<PlayFMODEventInstance>
{
    public override void Process(PlayFMODEventInstance packet)
    {
        Optional<GameObject> soundSource = NitroxEntity.GetObjectFrom(packet.Id);
        Validate.IsPresent(soundSource);

        FMODEmitterController fmodEmitterController = soundSource.Value.GetComponent<FMODEmitterController>();
        Validate.IsTrue(fmodEmitterController);

        if (packet.Play)
        {
            fmodEmitterController.PlayEventInstance(packet.AssetPath, packet.Volume, packet.Id);
        }
        else
        {
            fmodEmitterController.StopEventInstance(packet.AssetPath, packet.Id);
        }
    }
}
