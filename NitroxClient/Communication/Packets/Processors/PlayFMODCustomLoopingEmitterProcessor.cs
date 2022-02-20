using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayFMODCustomLoopingEmitterProcessor : ClientPacketProcessor<PlayFMODCustomLoopingEmitter>
{
    public override void Process(PlayFMODCustomLoopingEmitter packet)
    {
        GameObject soundSource = NitroxEntity.RequireObjectFrom(packet.Id);
        FMODEmitterController fmodEmitterController = soundSource.RequireComponent<FMODEmitterController>();

        fmodEmitterController.PlayCustomLoopingEmitter(packet.AssetPath);
    }
}
