using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    /// <summary>
    /// <para>
    /// The Cyclops damage logic is handled in a lot of spots. The Cyclops has its damage managed via a whopping 3 managers, all of which modify the health of the ship.
    /// </para><para>
    /// Cyclops health itself has logic held in <see cref="SubRoot.OnTakeDamage(DamageInfo)"/> and <see cref="LiveMixin.TakeDamage(float, Vector3, DamageType, GameObject)"/>.
    /// The <see cref="LiveMixin"/> is located at <see cref="SubRoot.live"/>. This check is done first. The rest of the logic will not run if the ship is shielded, or has been
    /// destroyed.
    /// </para><para>
    /// Cyclops hull damage points management is done in <see cref="CyclopsExternalDamageManager.OnTakeDamage"/> that's called via <see cref="SubRoot.OnTakeDamage(DamageInfo)"/>.
    /// The mixin is at <see cref="CyclopsExternalDamageManager.subLiveMixin"/>, and it looks to be a reference of <see cref="SubRoot.live"/>. This logic will always execute
    /// the fire logic.
    /// </para><para>
    /// Repair Logic is handled at <see cref="CyclopsExternalDamageManager.RepairPoint(CyclopsDamagePoint)"/> and <see cref="CyclopsDamagePoint.OnRepair"/>.
    /// </para><para>
    /// Cyclops fire damage management is done in <see cref="SubFire"/> located in <see cref="CyclopsExternalDamageManager.subFire"/>. They do not have any dependencies on each other,
    /// but I may be wrong. The <see cref="LiveMixin"/> is at <see cref="SubFire.liveMixin"/>, and looks to be a reference of <see cref="SubRoot.live"/>. If the health is below 80%,
    /// there's a chance a fire will start.
    /// </para><para>
    /// Cyclops fire extinguishing is handled at <see cref="Fire.Douse(float)"/>. A large douse amount triggers the private <see cref="Fire.Extinguish()"/> method.
    /// </para><para>
    /// Currently this does a full re-sync of damage points and fires due to having no set owner. Any client can call for an change of the Cyclops' health state.
    /// </para>
    /// </summary>
    public class CyclopsDamageProcessor : ClientPacketProcessor<CyclopsDamage>
    {
        private readonly IPacketSender packetSender;

        public CyclopsDamageProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsDamage packet)
        {
            SubRoot cyclops = GuidHelper.RequireObjectFrom(packet.Guid).GetComponent<SubRoot>();

            Multiplayer.Logic.Cyclops.SetActiveDamagePoints(cyclops, packet.DamagePointIndexes,
                packet.SubHealth,
                packet.DamageManagerHealth,
                packet.SubFireHealth);

            Multiplayer.Logic.Cyclops.SetActiveRoomFires(cyclops, packet.RoomFires,
                packet.SubHealth,
                packet.DamageManagerHealth,
                packet.SubFireHealth);
        }
    }
}
