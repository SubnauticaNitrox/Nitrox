using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;
using static NitroxModel.DisplayStatusCodes;
namespace NitroxClient.Communication.Packets.Processors;

public class FMODEventInstanceProcessor : ClientPacketProcessor<FMODEventInstancePacket>
{
    public override void Process(FMODEventInstancePacket packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject emitterControllerObject))
        {
            Log.ErrorOnce($"[{nameof(FMODEventInstanceProcessor)}] Couldn't find entity {packet.Id}");
            DisplayStatusCode(StatusCode.INVALID_PACKET, $"[{nameof(FMODEventInstanceProcessor)}] Couldn't find entity {packet.Id}");
            return;
        }

        if (!emitterControllerObject.TryGetComponent(out FMODEmitterController fmodEmitterController))
        {
            fmodEmitterController = emitterControllerObject.AddComponent<FMODEmitterController>();
            fmodEmitterController.LateRegisterEmitter();
        }

        if (packet.Play)
        {
            fmodEmitterController.PlayEventInstance(packet.AssetPath, packet.Volume);
        }
        else
        {
            fmodEmitterController.StopEventInstance(packet.AssetPath);
        }
    }
}
