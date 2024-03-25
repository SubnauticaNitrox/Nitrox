using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;
using static NitroxModel.DisplayStatusCodes;
namespace NitroxClient.Communication.Packets.Processors;

public class FMODCustomEmitterProcessor : ClientPacketProcessor<FMODCustomEmitterPacket>
{
    public override void Process(FMODCustomEmitterPacket packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject emitterControllerEntity))
        {
            Log.ErrorOnce($"[{nameof(FMODCustomEmitterProcessor)}] Couldn't find entity {packet.Id}");
            DisplayStatusCode(StatusCode.INVALID_PACKET, $"[{nameof(FMODCustomEmitterProcessor)}] Couldn't find entity {packet.Id}");
            return;
        }

        if (!emitterControllerEntity.TryGetComponent(out FMODEmitterController fmodEmitterController))
        {
            fmodEmitterController = emitterControllerEntity.AddComponent<FMODEmitterController>();
            fmodEmitterController.LateRegisterEmitter();
        }

        using (PacketSuppressor<FMODCustomEmitterPacket>.Suppress())
        using (PacketSuppressor<FMODCustomLoopingEmitterPacket>.Suppress())
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
