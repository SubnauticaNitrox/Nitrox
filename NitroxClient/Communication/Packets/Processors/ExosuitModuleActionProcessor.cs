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
    public class ExosuitModuleActionProcessor : ClientPacketProcessor<ExosuitModulesAction>
    {
        private readonly IPacketSender packetSender;

        public ExosuitModuleActionProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }
        public override void Process(ExosuitModulesAction packet)
        {
            Log.Info("TORPEDO PACKET");
            using (packetSender.Suppress<ExosuitModulesAction>())
            using (packetSender.Suppress<ItemContainerRemove>())
            {
                GameObject _gameObject = GuidHelper.RequireObjectFrom(packet.Guid);
                ExosuitTorpedoArm exosuit = _gameObject.GetComponent<ExosuitTorpedoArm>();
                if (exosuit != null)
                {
                        Transform muzzle = packet.SiloTransform;
                        Log.Info("TORPEDO SHOT:");
                        TorpedoType torpedoType = packet.TorpedoType;
   
                        //Original Function use Player Camera need parse owner camera values
                        TorpedoShot(torpedoType, muzzle, packet.Forward, packet.Rotation);
                }
            }
        }

        //Copied this from the Vehicle class
        public static bool TorpedoShot(TorpedoType torpedoType, Transform muzzle, bool verbose)
        {
            Log.Info("TORPEDO SHOOTING");
            if (torpedoType != null)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(torpedoType.prefab);
                Transform component = gameObject.GetComponent<Transform>();
                ExosuitTorpedoArm component2 = gameObject.GetComponent<ExosuitTorpedoArm>();
                Vector3 zero = Vector3.zero;
                Rigidbody componentInParent = muzzle.GetComponentInParent<Rigidbody>();
                Vector3 rhs = (!(componentInParent != null)) ? Vector3.zero : componentInParent.velocity;
                component2.Shoot(torpedoType, muzzle.position, verbose);
                return true;
            }
            return false;
        }
    }
}
