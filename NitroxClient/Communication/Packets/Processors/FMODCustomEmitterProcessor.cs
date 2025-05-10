using NitroxClient.MonoBehaviours;
using NitroxModel.Networking.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class FMODCustomEmitterProcessor : IClientPacketProcessor<FMODCustomEmitterPacket>
{
    public Task Process(IPacketProcessContext context, FMODCustomEmitterPacket packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject emitterControllerEntity))
        {
            Log.ErrorOnce($"[{nameof(FMODCustomEmitterProcessor)}] Couldn't find entity {packet.Id}");
            return Task.CompletedTask;
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

        return Task.CompletedTask;
    }
}
