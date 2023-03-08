using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
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
            using (PacketSuppressor<SeamothModulesAction>.Suppress())
            {
                GameObject _gameObject = NitroxEntity.RequireObjectFrom(packet.Id);
                SeaMoth seamoth = _gameObject.GetComponent<SeaMoth>();
                if (seamoth != null)
                {
                    TechType techType = packet.TechType.ToUnity();

                    if (techType == TechType.SeamothElectricalDefense)
                    {
                        float[] chargearray = seamoth.quickSlotCharge;
                        float charge = chargearray[packet.SlotID];
                        float slotCharge = seamoth.GetSlotCharge(packet.SlotID);
                        GameObject gameObject = global::Utils.SpawnZeroedAt(seamoth.seamothElectricalDefensePrefab, seamoth.transform, false);
                        ElectricalDefense component = gameObject.GetComponent<ElectricalDefense>();
                        component.charge = charge;
                        component.chargeScalar = slotCharge;
                    }

                    if (techType == TechType.SeamothTorpedoModule)
                    {
                        Transform muzzle = (packet.SlotID != seamoth.GetSlotIndex("SeamothModule1") && packet.SlotID != seamoth.GetSlotIndex("SeamothModule3")) ? seamoth.torpedoTubeRight : seamoth.torpedoTubeLeft;
                        ItemsContainer storageInSlot = seamoth.GetStorageInSlot(packet.SlotID, TechType.SeamothTorpedoModule);
                        TorpedoType torpedoType = null;

                        for (int i = 0; i < seamoth.torpedoTypes.Length; i++)
                        {
                            if (storageInSlot.Contains(seamoth.torpedoTypes[i].techType))
                            {
                                torpedoType = seamoth.torpedoTypes[i];
                                break;
                            }
                        }

                        //Original Function use Player Camera need parse owner camera values
                        TorpedoShot(storageInSlot, torpedoType, muzzle, packet.Forward.ToUnity(), packet.Rotation.ToUnity());
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
                SeamothTorpedo component2 = gameObject.GetComponent<SeamothTorpedo>();
                Vector3 zero = Vector3.zero;
                Rigidbody componentInParent = muzzle.GetComponentInParent<Rigidbody>();
                Vector3 rhs = (!(componentInParent != null)) ? Vector3.zero : componentInParent.velocity;
                float speed = Vector3.Dot(forward, rhs);
                component2.Shoot(muzzle.position, rotation, speed, -1f);

                return true;
            }

            return false;
        }
    }
}
