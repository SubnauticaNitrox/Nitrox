using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public sealed class SeamothModuleActionProcessor : ClientPacketProcessor<SeamothModulesAction>
{
    public override void Process(SeamothModulesAction packet)
    {
        using (PacketSuppressor<SeamothModulesAction>.Suppress())
        {
            if (!NitroxEntity.TryGetComponentFrom(packet.Id, out SeaMoth seamoth))
            {
                Log.Error($"[{nameof(SeamothModuleActionProcessor)}] Couldn't find SeaMoth component on {packet.Id}");
                return;
            }

            switch (packet.TechType.ToUnity())
            {
                case TechType.SeamothElectricalDefense:
                {
                    float[] chargeArray = seamoth.quickSlotCharge;
                    float charge = chargeArray[packet.SlotID];
                    float slotCharge = seamoth.GetSlotCharge(packet.SlotID);

                    GameObject gameObject = Utils.SpawnZeroedAt(seamoth.seamothElectricalDefensePrefab, seamoth.transform, false);
                    ElectricalDefense component = gameObject.GetComponent<ElectricalDefense>();
                    component.charge = charge;
                    component.chargeScalar = slotCharge;
                    component.defenseSound = null; // Disable sound in Start(). Sound is synced over general Nitrox FMOD system.
                    break;
                }
            }
        }
    }
}
