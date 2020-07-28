using System.ComponentModel;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class SeamothModuleActionProcessor : ClientPacketProcessor<SeamothModulesAction>
    {
        private readonly IPacketSender packetSender;

        public SeamothModuleActionProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }
        public override void Process(SeamothModulesAction packet)
        {
            using (packetSender.Suppress<SeamothModulesAction>())
            using (packetSender.Suppress<ItemContainerRemove>())
            {
                SeaMoth seamoth = NitroxEntity.RequireObjectFrom(packet.Id).GetComponent<SeaMoth>();
                if (seamoth)
                {
                    switch (packet.TechType.ToUnity())
                    {
                        case TechType.SeamothElectricalDefense:
                            {
                                float[] chargeArray = (float[])seamoth.ReflectionGet("quickSlotCharge");
                                float charge = chargeArray[packet.SlotID];
                                float slotCharge = seamoth.GetSlotCharge(packet.SlotID);
                                GameObject gameObject = Utils.SpawnZeroedAt(seamoth.seamothElectricalDefensePrefab, seamoth.transform);
                                ElectricalDefense component = gameObject.GetComponent<ElectricalDefense>();
                                component.charge = charge;
                                component.chargeScalar = slotCharge;
                                break;
                            }
                        case TechType.SeamothTorpedoModule:
                            {
                                Transform muzzle = (packet.SlotID != seamoth.GetSlotIndex("SeamothModule1") && packet.SlotID != seamoth.GetSlotIndex("SeamothModule3")) ? seamoth.torpedoTubeRight : seamoth.torpedoTubeLeft;
                                ItemsContainer storageInSlot = seamoth.GetStorageInSlot(packet.SlotID, TechType.SeamothTorpedoModule);
                                TorpedoType torpedoType = null;

                                foreach (TorpedoType type in seamoth.torpedoTypes)
                                {
                                    if (storageInSlot.Contains(type.techType))
                                    {
                                        torpedoType = type;
                                        break;
                                    }
                                }

                                //Original Function use Player Camera need parse owner camera values
                                TorpedoShot(storageInSlot, torpedoType, muzzle, packet.Forward.ToUnity(), packet.Rotation.ToUnity());
                                break;
                            }
                        default:
                            throw new InvalidEnumArgumentException($"{packet.TechType.ToUnity()} is not supported in {nameof(SeamothModuleActionProcessor)}");
                    }
                }
            }
        }

        //Inspired by the Vehicle class
        private static void TorpedoShot(ItemsContainer container, TorpedoType torpedoType, Transform muzzle, Vector3 forward, Quaternion rotation)
        {
            if (torpedoType != null && container.DestroyItem(torpedoType.techType))
            {
                SeamothTorpedo seamothTorpedo = Object.Instantiate(torpedoType.prefab).GetComponent<SeamothTorpedo>();
                Rigidbody rigidbody = muzzle.GetComponentInParent<Rigidbody>();
                Vector3 velocity = rigidbody ? Vector3.zero : rigidbody.velocity;
                seamothTorpedo.Shoot(muzzle.position, rotation, Vector3.Dot(forward, velocity), -1f);
            }
        }
    }
}
