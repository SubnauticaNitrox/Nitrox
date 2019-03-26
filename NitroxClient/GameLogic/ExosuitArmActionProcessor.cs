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
            if(packet.TechType == TechType.ExosuitClawArmModule)
            {
                exosuitModuleEvent.UseClaw(gameObject.GetComponent<ExosuitClawArm>(), packet.ArmAction);
            }
        }
    }
}
