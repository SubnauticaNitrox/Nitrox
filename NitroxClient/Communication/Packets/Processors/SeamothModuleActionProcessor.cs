#if SUBNAUTICA
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
                }
            }
        }
    }
}
#endif
