using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    /// <summary>
    /// Add/remove <see cref="CyclopsDamagePoint"/>s and <see cref="Fire"/>s to match the <see cref="CyclopsDamage"/> packet received
    /// </summary>
    public class CyclopsDamageProcessor : ClientPacketProcessor<CyclopsDamage>
    {
        private readonly IPacketSender packetSender;
        private readonly Fires fires;

        public CyclopsDamageProcessor(IPacketSender packetSender, Fires fires)
        {
            this.packetSender = packetSender;
            this.fires = fires;
        }

        public override void Process(CyclopsDamage packet)
        {
            SubRoot subRoot = NitroxEntity.RequireObjectFrom(packet.Id).GetComponent<SubRoot>();

            using (PacketSuppressor<CyclopsDamagePointRepaired>.Suppress())
            {
                SetActiveDamagePoints(subRoot, packet.DamagePointIndexes);
            }

            using (PacketSuppressor<FireDoused>.Suppress())
            {
                SetActiveRoomFires(subRoot, packet.RoomFires);
            }

            LiveMixin subHealth = subRoot.gameObject.RequireComponent<LiveMixin>();

            float oldHPPercent = subRoot.oldHPPercent;

            // Client side noises. Not necessary for keeping the health synced
            if (subHealth.GetHealthFraction() < 0.5f && oldHPPercent >= 0.5f)
            {
                subRoot.voiceNotificationManager.PlayVoiceNotification(subRoot.hullLowNotification, true, false);
            }
            else if (subHealth.GetHealthFraction() < 0.25f && oldHPPercent >= 0.25f)
            {
                subRoot.voiceNotificationManager.PlayVoiceNotification(subRoot.hullCriticalNotification, true, false);
            }

            using (PacketSuppressor<CyclopsDamage>.Suppress())
            {
                // Not necessary, but used by above code whenever damage is done
                subRoot.oldHPPercent = subHealth.GetHealthFraction();

                // Apply the actual health changes
                subRoot.gameObject.RequireComponent<LiveMixin>().health = packet.SubHealth;
                subRoot.gameObject.RequireComponentInChildren<CyclopsExternalDamageManager>().subLiveMixin.health = packet.DamageManagerHealth;
                subRoot.gameObject.RequireComponent<SubFire>().liveMixin.health = packet.SubFireHealth;
            }
        }

        /// <summary>
        /// Add/remove <see cref="CyclopsDamagePoint"/>s until it matches the <paramref name="damagePointIndexes"/> array passed. Can trigger <see cref="CyclopsDamagePointRepaired"/> packets
        /// </summary>
        private void SetActiveDamagePoints(SubRoot cyclops, int[] damagePointIndexes)
        {
            CyclopsExternalDamageManager damageManager = cyclops.gameObject.RequireComponentInChildren<CyclopsExternalDamageManager>();
            List<CyclopsDamagePoint> unusedDamagePoints = damageManager.unusedDamagePoints;

            // CyclopsExternalDamageManager.damagePoints is an unchanged list. It will never have items added/removed from it. Since packet.DamagePointIndexes is also an array
            // generated in an ordered manner, we can match them without worrying about unordered items.
            if (damagePointIndexes != null && damagePointIndexes.Length > 0)
            {
                int packetDamagePointsIndex = 0;

                for (int damagePointsIndex = 0; damagePointsIndex < damageManager.damagePoints.Length; damagePointsIndex++)
                {
                    // Loop over all of the packet.DamagePointIndexes as long as there's more to match
                    if (packetDamagePointsIndex < damagePointIndexes.Length
                        && damagePointIndexes[packetDamagePointsIndex] == damagePointsIndex)
                    {
                        if (!damageManager.damagePoints[damagePointsIndex].gameObject.activeSelf)
                        {
                            // Copied from CyclopsExternalDamageManager.CreatePoint(), except without the random index pick.
                            damageManager.damagePoints[damagePointsIndex].gameObject.SetActive(true);
                            damageManager.damagePoints[damagePointsIndex].RestoreHealth();
                            GameObject prefabGo = damageManager.fxPrefabs[UnityEngine.Random.Range(0, damageManager.fxPrefabs.Length - 1)];
                            damageManager.damagePoints[damagePointsIndex].SpawnFx(prefabGo);
                            unusedDamagePoints.Remove(damageManager.damagePoints[damagePointsIndex]);
                        }

                        packetDamagePointsIndex++;
                    }
                    else
                    {
                        // If it's active, but not in the list, it must have been repaired.
                        if (damageManager.damagePoints[damagePointsIndex].gameObject.activeSelf)
                        {
                            RepairDamagePoint(cyclops, damagePointsIndex, 999);
                        }
                    }
                }

                // Looks like the list came in unordered. I've uttered "That shouldn't happen" enough to do sanity checks for what should be impossible.
                if (packetDamagePointsIndex < damagePointIndexes.Length)
                {
                    Log.Error($"[CyclopsDamageProcessor packet.DamagePointIds did not fully iterate! Id: {damagePointIndexes[packetDamagePointsIndex]} had no matching Id in damageManager.damagePoints, or the order is incorrect!]");
                }
            }
            else
            {
                // None should be active.
                for (int i = 0; i < damageManager.damagePoints.Length; i++)
                {
                    if (damageManager.damagePoints[i].gameObject.activeSelf)
                    {
                        RepairDamagePoint(cyclops, i, 999);
                    }
                }
            }

            // unusedDamagePoints is checked against damagePoints to determine if there's enough damage points. Failing to set the new list
            // of unusedDamagePoints will cause random DamagePoints to appear.
            damageManager.unusedDamagePoints = unusedDamagePoints;
            // Visual update only to show the water leaking through the window and various hull points based on missing health.
            damageManager.ToggleLeakPointsBasedOnDamage();
        }

        /// <summary>
        /// Add/remove fires until it matches the <paramref name="roomFires"/> array. Can trigger <see cref="FireDoused"/> packets
        /// </summary>
        private void SetActiveRoomFires(SubRoot subRoot, CyclopsFireData[] roomFires)
        {
            SubFire subFire = subRoot.gameObject.RequireComponent<SubFire>();
            Dictionary<CyclopsRooms, SubFire.RoomFire> roomFiresDict = subFire.roomFires;
            NitroxId subRootId = NitroxEntity.GetId(subRoot.gameObject);
            CyclopsFireData fireNode = null;

            if (roomFires != null && roomFires.Length > 0)
            {
                // Removing and adding fires will happen in the same loop
                foreach (KeyValuePair<CyclopsRooms, SubFire.RoomFire> keyValuePair in roomFiresDict)
                {
                    for (int nodeIndex = 0; nodeIndex < keyValuePair.Value.spawnNodes.Length; nodeIndex++)
                    {
                        fireNode = roomFires.SingleOrDefault(x => x.Room == keyValuePair.Key && x.NodeIndex == nodeIndex);

                        // If there's a matching node index, add a fire if there isn't one already. Otherwise remove a fire if there is one
                        if (fireNode == null)
                        {
                            if (keyValuePair.Value.spawnNodes[nodeIndex].childCount > 0)
                            {
                                keyValuePair.Value.spawnNodes[nodeIndex].GetComponentInChildren<Fire>().Douse(10000);
                            }
                        }
                        else
                        {
                            if (keyValuePair.Value.spawnNodes[nodeIndex].childCount < 1)
                            {
                                fires.Create(new CyclopsFireData(fireNode.FireId, subRootId, fireNode.Room, fireNode.NodeIndex));
                            }
                        }
                    }
                }
            }
            // Clear out the fires, there should be none active
            else
            {
                foreach (KeyValuePair<CyclopsRooms, SubFire.RoomFire> keyValuePair in roomFiresDict)
                {
                    for (int nodeIndex = 0; nodeIndex < keyValuePair.Value.spawnNodes.Length; nodeIndex++)
                    {
                        if (keyValuePair.Value.spawnNodes[nodeIndex].childCount > 0)
                        {
                            keyValuePair.Value.spawnNodes[nodeIndex].GetComponentInChildren<Fire>().Douse(10000);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Set the health of a <see cref="CyclopsDamagePoint"/>. This can trigger sending <see cref="CyclopsDamagePointRepaired"/> packets
        /// </summary>
        /// <param name="repairAmount">The max health of the point is 1. 999 is passed to trigger a full repair of the <see cref="CyclopsDamagePoint"/></param>
        private void RepairDamagePoint(SubRoot subRoot, int damagePointIndex, float repairAmount)
        {
            subRoot.damageManager.damagePoints[damagePointIndex].liveMixin.AddHealth(repairAmount);
        }
    }
}
