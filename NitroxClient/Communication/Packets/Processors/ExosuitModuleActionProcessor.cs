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
            using (packetSender.Suppress<ExosuitModulesAction>())
            using (packetSender.Suppress<ItemContainerRemove>())
            {
                GameObject _gameObject = GuidHelper.RequireObjectFrom(packet.Guid);
                Exosuit exosuit = _gameObject.GetComponent<Exosuit>();
                if (exosuit != null)
                {
                    if (packet.TechType == TechType.ExosuitTorpedoArmModule)
                    {
                        //Transform arm = exosuit.silo
                        ItemsContainer storageInSlot = exosuit.GetStorageInSlot(packet.SlotID, TechType.ExosuitTorpedoArmModule);
                        TorpedoType torpedoType = null;
                        for (int i = 0; i < exosuit.torpedoTypes.Length; i++)
                        {
                            if (storageInSlot.Contains(exosuit.torpedoTypes[i].techType))
                            {
                                torpedoType = exosuit.torpedoTypes[i];
                                break;
                            }
                        }
                        //Original Function use Player Camera need parse owner camera values
                        //TorpedoShot(storageInSlot, torpedoType, muzzle,packet.Forward,packet.Rotation);
                        

                    }
                }
            }
        }

        //Copied this from the Vehicle class
        public static bool TorpedoShot(ItemsContainer container, TorpedoType torpedoType, Transform muzzle, Vector3 forward, Quaternion rotation)
        {
            if (torpedoType != null && container.DestroyItem(torpedoType.techType))
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(torpedoType.prefab);
                Transform component = gameObject.GetComponent<Transform>();
                ExosuitTorpedoArm component2 = gameObject.GetComponent<ExosuitTorpedoArm>();
                Vector3 zero = Vector3.zero;
                Rigidbody componentInParent = muzzle.GetComponentInParent<Rigidbody>();
                Vector3 rhs = (!(componentInParent != null)) ? Vector3.zero : componentInParent.velocity;
                float speed = Vector3.Dot(forward, rhs);
                //component2.siloFirst. Try(muzzle.position, rotation, speed, -1f);
                return true;
            }
            return false;
        }
    }
}
