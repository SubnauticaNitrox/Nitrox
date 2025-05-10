using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.Subnautica.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class ExosuitArmActionProcessor : IClientPacketProcessor<ExosuitArmActionPacket>
{
    public Task Process(IPacketProcessContext context, ExosuitArmActionPacket packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.ArmId, out GameObject gameObject))
        {
            Log.Error("Could not find exosuit arm");
            return Task.CompletedTask;
        }

        switch (packet.TechType)
        {
            case TechType.ExosuitClawArmModule:
                ExosuitModuleEvent.UseClaw(gameObject.GetComponent<ExosuitClawArm>(), packet.ArmAction);
                break;
            case TechType.ExosuitDrillArmModule:
                ExosuitModuleEvent.UseDrill(gameObject.GetComponent<ExosuitDrillArm>(), packet.ArmAction);
                break;
            case TechType.ExosuitGrapplingArmModule:
                ExosuitModuleEvent.UseGrappling(gameObject.GetComponent<ExosuitGrapplingArm>(), packet.ArmAction, packet.OpVector?.ToUnity());
                break;
            default:
                Log.Error($"Got an arm tech that is not handled: {packet.TechType} with action: {packet.ArmAction} for id {packet.ArmId}");
                break;
        }

        return Task.CompletedTask;
    }
}
