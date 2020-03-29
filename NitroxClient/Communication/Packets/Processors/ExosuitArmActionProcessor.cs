using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ExosuitArmActionProcessor : ClientPacketProcessor<ExosuitArmActionPacket>
    {
        private readonly IPacketSender packetSender;
        private readonly ExosuitModuleEvent exosuitModuleEvent;

        public ExosuitArmActionProcessor(IPacketSender packetSender, ExosuitModuleEvent exosuitModuleEvent)
        {
            this.packetSender = packetSender;
            this.exosuitModuleEvent = exosuitModuleEvent;
        }

        public override void Process(ExosuitArmActionPacket packet)
        {
            Optional<GameObject> opGameObject = NitroxEntity.GetObjectFrom(packet.ArmId);
            if (!opGameObject.HasValue)
            {
                Log.Error("Could not find exosuit arm");
                return;
            }
            GameObject gameObject = opGameObject.Value;
            switch (packet.TechType)
            {

                case TechType.ExosuitClawArmModule:
                    exosuitModuleEvent.UseClaw(gameObject.GetComponent<ExosuitClawArm>(), packet.ArmAction);
                    break;
                case TechType.ExosuitDrillArmModule:
                    exosuitModuleEvent.UseDrill(gameObject.GetComponent<ExosuitDrillArm>(), packet.ArmAction);
                    break;
                case TechType.ExosuitGrapplingArmModule:
                    exosuitModuleEvent.UseGrappling(gameObject.GetComponent<ExosuitGrapplingArm>(), packet.ArmAction, packet.OpVector);
                    break;
                case TechType.ExosuitTorpedoArmModule:                    
                        exosuitModuleEvent.UseTorpedo(gameObject.GetComponent<ExosuitTorpedoArm>(), packet.ArmAction, packet.OpVector, packet.OpRotation);                    
                    break;
                default:
                    Log.Error("Got an arm tech that is not handled: " + packet.TechType + " with action: " + packet.ArmAction + " for id " + packet.ArmId);
                    break;
            }
            
        }
    }
}
