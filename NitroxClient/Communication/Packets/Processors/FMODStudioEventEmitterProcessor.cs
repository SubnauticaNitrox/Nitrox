using NitroxClient.MonoBehaviours;
using NitroxModel.Networking.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class FMODStudioEventEmitterProcessor : IClientPacketProcessor<FMODStudioEmitterPacket>
{
    public Task Process(IPacketProcessContext context, FMODStudioEmitterPacket packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject emitterControllerObject))
        {
            Log.ErrorOnce($"[{nameof(FMODStudioEventEmitterProcessor)}] Couldn't find entity {packet.Id}");
            return Task.CompletedTask;
        }

        if (!emitterControllerObject.TryGetComponent(out FMODEmitterController fmodEmitterController))
        {
            fmodEmitterController = emitterControllerObject.AddComponent<FMODEmitterController>();
            fmodEmitterController.LateRegisterEmitter();
        }

        using (PacketSuppressor<FMODStudioEmitterPacket>.Suppress())
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

        return Task.CompletedTask;
    }
}
