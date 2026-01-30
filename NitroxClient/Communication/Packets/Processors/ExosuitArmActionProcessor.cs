using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class ExosuitArmActionProcessor : IClientPacketProcessor<ExosuitArmActionPacket>
{
    public Task Process(ClientProcessorContext context, ExosuitArmActionPacket packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.ExosuitId, out GameObject gameObject))
        {
            Log.Error("Could not find exosuit for arm action");
            return Task.CompletedTask;
        }

        Exosuit exosuit = gameObject.RequireComponent<Exosuit>();
        IExosuitArm arm = packet.ArmSide == Exosuit.Arm.Left ? exosuit.leftArm : exosuit.rightArm;

        switch (packet.TechType)
        {
            case TechType.ExosuitClawArmModule when arm is ExosuitClawArm clawArm:
                ExosuitModuleEvent.UseClaw(clawArm, packet.ArmAction);
                break;
            case TechType.ExosuitDrillArmModule when arm is ExosuitDrillArm drillArm:
                ExosuitModuleEvent.UseDrill(drillArm, packet.ArmAction);
                break;
            case TechType.ExosuitGrapplingArmModule when arm is ExosuitGrapplingArm grapplingArm:
                ExosuitModuleEvent.UseGrappling(grapplingArm, packet.ArmAction);
                break;
            case TechType.ExosuitPropulsionArmModule when arm is ExosuitPropulsionArm propulsionArm:
                ExosuitModuleEvent.UsePropulsion(propulsionArm, packet.ArmAction);
                break;
            case TechType.ExosuitTorpedoArmModule when arm is ExosuitTorpedoArm torpedoArm:
                ExosuitModuleEvent.UseTorpedo(torpedoArm, packet.ArmAction);
                break;
            default:
                Log.Error($"Unhandled arm tech or invalid arm type: {packet.TechType} with action {packet.ArmAction} on {arm.GetGameObject().name} for exosuit {packet.ExosuitId}");
                break;
        }
        return Task.CompletedTask;
    }
}
