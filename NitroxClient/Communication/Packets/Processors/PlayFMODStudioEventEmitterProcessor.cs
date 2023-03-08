using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayFMODStudioEventEmitterProcessor : ClientPacketProcessor<PlayFMODStudioEmitter>
{
    private readonly IPacketSender packetSender;

    public PlayFMODStudioEventEmitterProcessor(IPacketSender packetSender)
    {
        this.packetSender = packetSender;
    }


    public override void Process(PlayFMODStudioEmitter packet)
    {
        GameObject soundSource = NitroxEntity.RequireObjectFrom(packet.Id);
        FMODEmitterController fmodEmitterController = soundSource.RequireComponent<FMODEmitterController>();

        using (PacketSuppressor<PlayFMODStudioEmitter>.Suppress())
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
