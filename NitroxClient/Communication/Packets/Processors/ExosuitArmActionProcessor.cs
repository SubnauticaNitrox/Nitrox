using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class ExosuitArmActionProcessor : ClientPacketProcessor<ExosuitArmActionPacket>
{
    public override void Process(ExosuitArmActionPacket packet)
    {
        if (!NitroxEntity.TryGetObjectFrom(packet.ExosuitId, out GameObject gameObject))
        {
            Log.Error("Could not find exosuit for arm action");
            return;
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
    }
}
