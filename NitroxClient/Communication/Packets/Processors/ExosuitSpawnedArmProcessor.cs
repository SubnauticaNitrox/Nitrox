using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ExosuitSpawnedArmProcessor : ClientPacketProcessor<ExosuitSpawnedArmAction>
    {
        private readonly IPacketSender packetSender;

        public ExosuitSpawnedArmProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }
        public override void Process(ExosuitSpawnedArmAction packet)
        {
            using (packetSender.Suppress<ExosuitSpawnedArmAction>())
            {
                GameObject _gameObject = GuidHelper.RequireObjectFrom(packet.ExoGuid);
                Exosuit exosuit = _gameObject.GetComponent<Exosuit>();

                IExosuitArm leftArm = (IExosuitArm)exosuit.ReflectionGet("leftArm");
                IExosuitArm rightArm = (IExosuitArm)exosuit.ReflectionGet("rightArm");

                GameObject rightArmOb = rightArm.GetGameObject();
                ExosuitClawArm rightClawArm = rightArmOb.GetComponent<ExosuitClawArm>();

                GameObject leftArmOb = leftArm.GetGameObject();
                ExosuitClawArm leftClawArm = leftArmOb.GetComponent<ExosuitClawArm>();

                rightArmOb.SetNewGuid(packet.RightArmGuid);
                leftArmOb.SetNewGuid(packet.LeftArmGuid);
            }
        }
    }
}
