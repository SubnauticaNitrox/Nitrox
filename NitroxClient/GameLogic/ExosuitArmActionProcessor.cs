using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
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
            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(packet.ArmGuid);
            if(opGameObject.IsEmpty())
            {
                Log.Debug("Could not find exosuit arm");
                return;
            }
            GameObject gameObject = opGameObject.Get();
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
                default:
                    Log.Error("Got an arm tech that is not handled: " + packet.TechType + " with action: " + packet.ArmAction + " for guid " + packet.ArmGuid);
                    break;
            }
            
        }
    }
}
