using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class FMODStudioEventEmitterProcessor : IClientPacketProcessor<FMODStudioEmitterPacket>
{
    public Task Process(ClientProcessorContext context, FMODStudioEmitterPacket packet)
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
                fmodEmitterController.PlayStudioEmitter(packet.AssetPath, emitterControllerObject.transform.position);
            }
            else
            {
                fmodEmitterController.StopStudioEmitter(packet.AssetPath, packet.AllowFadeout);
            }
        }
        return Task.CompletedTask;
    }
}
