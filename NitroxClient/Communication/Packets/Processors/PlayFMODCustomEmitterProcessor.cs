using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayFMODCustomEmitterProcessor : ClientPacketProcessor<PlayFMODCustomEmitter>
{
    private readonly IPacketSender packetSender;

    public PlayFMODCustomEmitterProcessor(IPacketSender packetSender)
    {
        this.packetSender = packetSender;
    }


    public override void Process(PlayFMODCustomEmitter packet)
    {
        GameObject soundSource = NitroxEntity.RequireObjectFrom(packet.Id);
        FMODEmitterController fmodEmitterController = soundSource.RequireComponent<FMODEmitterController>();

        using (PacketSuppressor<PlayFMODCustomEmitter>.Suppress())
        using (PacketSuppressor<PlayFMODCustomLoopingEmitter>.Suppress())
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
