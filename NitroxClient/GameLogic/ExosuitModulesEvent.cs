using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class ExosuitModulesEvent
    {
        private readonly IPacketSender packetSender;

        public ExosuitModulesEvent(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void BroadcastTorpedoLaunch(TorpedoType torpedoType, Transform siloTransform, bool verbose, ExosuitTorpedoArm instance)
        {
            Log.Info("TORPEDO EVENT");
            string Guid = GuidHelper.GetGuid(instance.gameObject);

            if (torpedoType != null) // Dont send packet if torpedo storage is empty
            {
                ExosuitModulesAction Changed = new ExosuitModulesAction(torpedoType, siloTransform, Guid, Player.main.camRoot.GetAimingTransform().forward, Player.main.camRoot.GetAimingTransform().rotation);
                packetSender.Send(Changed);
            }
        }
    }
}
