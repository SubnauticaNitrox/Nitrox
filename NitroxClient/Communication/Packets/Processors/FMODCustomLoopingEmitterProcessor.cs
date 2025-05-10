using NitroxClient.MonoBehaviours;
using NitroxModel.Networking.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class FMODCustomLoopingEmitterProcessor : IClientPacketProcessor<FMODCustomLoopingEmitterPacket>
{
    public Task Process(IPacketProcessContext context, FMODCustomLoopingEmitterPacket packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject emitterControllerObject))
        {
            Log.ErrorOnce($"[{nameof(FMODCustomLoopingEmitterProcessor)}] Couldn't find entity {packet.Id}");
            return Task.CompletedTask;
        }

        if (!emitterControllerObject.TryGetComponent(out FMODEmitterController fmodEmitterController))
        {
            fmodEmitterController = emitterControllerObject.AddComponent<FMODEmitterController>();
            fmodEmitterController.LateRegisterEmitter();
        }

        fmodEmitterController.PlayCustomLoopingEmitter(packet.AssetPath);

        return Task.CompletedTask;
    }
}
