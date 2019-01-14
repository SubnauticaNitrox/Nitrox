using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsDamageProcessor : ClientPacketProcessor<CyclopsDamage>
    {
        private readonly IPacketSender packetSender;

        public CyclopsDamageProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsDamage packet)
        {
            SubRoot subRoot = GuidHelper.RequireObjectFrom(packet.Guid).GetComponent<SubRoot>();

            using (packetSender.Suppress<CyclopsDamagePointHealthChanged>())
            {
                SetActiveDamagePoints(subRoot, packet.DamagePointIndexes);
            }

            using (packetSender.Suppress<CyclopsFireHealthChanged>())
            {
                SetActiveRoomFires(subRoot, packet.RoomFires);
            }

            LiveMixin subHealth = subRoot.gameObject.RequireComponent<LiveMixin>();

            float oldHPPercent = (float)subRoot.ReflectionGet("oldHPPercent");

            if (subHealth.GetHealthFraction() < 0.5f && oldHPPercent >= 0.5f)
            {
                subRoot.voiceNotificationManager.PlayVoiceNotification(subRoot.hullLowNotification, true, false);
            }
            else if (subHealth.GetHealthFraction() < 0.25f && oldHPPercent >= 0.25f)
            {
                subRoot.voiceNotificationManager.PlayVoiceNotification(subRoot.hullCriticalNotification, true, false);
            }

            using (packetSender.Suppress<CyclopsDamage>())
            {
                // Not necessary, but used by above code whenever damage is done
                subRoot.ReflectionSet("oldHPPercent", subHealth.GetHealthFraction());

                // Apply the actual health changes
                subRoot.gameObject.RequireComponent<LiveMixin>().health = packet.SubHealth;
                subRoot.gameObject.RequireComponentInChildren<CyclopsExternalDamageManager>().subLiveMixin.health = packet.DamageManagerHealth;
                subRoot.gameObject.RequireComponent<SubFire>().liveMixin.health = packet.SubFireHealth;
            }
        }

        /// <summary>
        /// Add/remove <see cref="CyclopsDamagePoint"/>s until it matches the <paramref name="damagePointIndexes"/> array passed
        /// </summary>
        private void SetActiveDamagePoints(SubRoot cyclops, int[] damagePointIndexes)
        {
            CyclopsExternalDamageManager damageManager = cyclops.gameObject.RequireComponentInChildren<CyclopsExternalDamageManager>();
            List<CyclopsDamagePoint> unusedDamagePoints = (List<CyclopsDamagePoint>)damageManager.ReflectionGet("unusedDamagePoints");

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
                    Log.Error("[CyclopsDamageProcessor packet.DamagePointGuids did not fully iterate! Guid: " + damagePointIndexes[packetDamagePointsIndex].ToString()
                        + " had no matching Guid in damageManager.damagePoints, or the order is incorrect!]");
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
            damageManager.ReflectionSet("unusedDamagePoints", unusedDamagePoints);
            // Visual update only to show the water leaking through the window and various hull points based on missing health.
            damageManager.ReflectionCall("ToggleLeakPointsBasedOnDamage", false, false, null);
        }

        /// <summary>
        /// Add/remove fires until it matches the <paramref name="roomFires"/> array.
        /// </summary>
        private void SetActiveRoomFires(SubRoot subRoot, SerializableRoomFire[] roomFires)
        {
            SubFire subFire = subRoot.gameObject.RequireComponent<SubFire>();
            Dictionary<CyclopsRooms, SubFire.RoomFire> roomFiresDict = (Dictionary<CyclopsRooms, SubFire.RoomFire>)subFire.ReflectionGet("roomFires");

            // We can do a look for fires that shouldn't be around by looping through GetAllFires.
            foreach (KeyValuePair<CyclopsRooms, SubFire.RoomFire> keyValuePair in roomFiresDict)
            {
                if (roomFires != null && roomFires.Any(x => x.Room == keyValuePair.Key))
                {
                    SerializableRoomFire roomFire = roomFires.FirstOrDefault(x => x.Room == keyValuePair.Key);

                    // There's fires that are supposed to be here.
                    for (int nodesIndex = 0; nodesIndex < keyValuePair.Value.spawnNodes.Length; nodesIndex++)
                    {
                        if (roomFire.ActiveRoomFireNodes.Any(x => x.NodeIndex == nodesIndex))
                        {
                            int fireCount = roomFire.ActiveRoomFireNodes.FirstOrDefault(x => x.NodeIndex == nodesIndex).FireCount;

                            // There's fewer fires than what there should be. Create new ones
                            if (keyValuePair.Value.spawnNodes[nodesIndex].childCount < fireCount)
                            {
                                // A while statement was locking the game. I'm figuring the process of creating a fire does not happen
                                // on the same thread.
                                int fireCountToAdd = fireCount - keyValuePair.Value.spawnNodes[nodesIndex].childCount;

                                for (int i = 0; i < fireCountToAdd; i++)
                                {
                                    CreateFire(subRoot, keyValuePair, nodesIndex);
                                }
                            }
                            else
                            {
                                if (keyValuePair.Value.spawnNodes[nodesIndex].childCount > fireCount)
                                {
                                    // A while statement was locking the game. I'm guessing calling a douse was doing something on a different thread
                                    // and the object wasn't getting destroyed before the loop checked again.
                                    int fireCountToRemove = keyValuePair.Value.spawnNodes[nodesIndex].childCount - fireCount;

                                    // I don't want to extinguish fires in the middle of the stack. The order can still matter. Note the conditions in the
                                    // for loop, it does not compare i to continue looping, it compares fireCountToRemove.
                                    for (int i = keyValuePair.Value.spawnNodes[nodesIndex].childCount - 1; fireCountToRemove > 0; i--)
                                    {
                                        DouseFire(subFire, keyValuePair.Key, nodesIndex, i, 10000);

                                        fireCountToRemove--;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // None for this node. Remove any that are there.
                            if (keyValuePair.Value.spawnNodes[nodesIndex].childCount > 0)
                            {
                                Fire[] fires = keyValuePair.Value.spawnNodes[nodesIndex].GetAllComponentsInChildren<Fire>();

                                for (int i = fires.Length - 1; i >= 0; i--)
                                {
                                    DouseFire(subFire, keyValuePair.Key, nodesIndex, i, 10000);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Remove all of the fires in here
                    for (int nodeIndex = 0; nodeIndex < keyValuePair.Value.spawnNodes?.Length; nodeIndex++)
                    {
                        if (keyValuePair.Value.spawnNodes[nodeIndex] != null && keyValuePair.Value.spawnNodes[nodeIndex].childCount > 0)
                        {
                            Fire[] fires = keyValuePair.Value.spawnNodes[nodeIndex].GetAllComponentsInChildren<Fire>();

                            for (int i = fires.Length - 1; i >= 0; i--)
                            {
                                DouseFire(subFire, keyValuePair.Key, nodeIndex, i, 10000);
                            }
                        }
                    }
                }
            }

            subFire.ReflectionSet("roomFires", roomFiresDict);
        }

        /// <summary>
        /// Create a new fire in a specific room and node. Calling <see cref="SubFire.CreateFire(SubFire.RoomFire)"/> would just cause it to use 
        /// a random number generator to choose the node. This can trigger sending <see cref="CyclopsDamage"/> packets
        /// </summary>
        private void CreateFire(SubRoot subRoot, KeyValuePair<CyclopsRooms, SubFire.RoomFire> fireRoom, int nodeIndex)
        {
            Transform transform2 = fireRoom.Value.spawnNodes[nodeIndex];
            fireRoom.Value.fireValue++;
            PrefabSpawn component = transform2.GetComponent<PrefabSpawn>();
            if (component == null)
            {
                return;
            }
            GameObject gameObject = component.SpawnManual();
            Fire componentInChildren = gameObject.GetComponentInChildren<Fire>();
            if (componentInChildren)
            {
                componentInChildren.fireSubRoot = subRoot;
            }
        }

        /// <summary>
        /// Set the health of a <see cref="CyclopsDamagePoint"/>. This can trigger sending <see cref="CyclopsDamagePointHealthChanged"/> packets
        /// </summary>
        /// <param name="repairAmount">The max health of the point is 1. 999 is passed to trigger a full repair of the <see cref="CyclopsDamagePoint"/></param>
        private void RepairDamagePoint(SubRoot subRoot, int damagePointIndex, float repairAmount)
        {
            subRoot.damageManager.damagePoints[damagePointIndex].liveMixin.AddHealth(repairAmount);
        }

        /// <summary>
        /// Executes the logic in <see cref="Fire.Douse(float)"/> while not calling the method itself to avoid endless looped calls. 
        /// If the fire is extinguished (no health left), it will pass a large float to trigger the private <see cref="Fire.Extinguish()"/> method.
        /// This can trigger sending <see cref="CyclopsFireHealthChanged"/> and <see cref="CyclopsDamage"/> packets
        /// </summary>
        private void DouseFire(SubFire fireManager, CyclopsRooms room, int fireTransformIndex, int fireIndex, float douseAmount)
        {
            Dictionary<CyclopsRooms, SubFire.RoomFire> roomFires = (Dictionary<CyclopsRooms, SubFire.RoomFire>)fireManager.ReflectionGet("roomFires");

            // What a beautiful monster this is.
            if (roomFires[room]?.spawnNodes[fireTransformIndex]?.GetAllComponentsInChildren<Fire>()?.Length <= fireIndex)
            {
                Log.Warn("[Cyclops.DouseFire fireIndex larger than number of Fires fireIndex: " + fireIndex.ToString()
                    + " Fire Count: " + roomFires[room]?.spawnNodes[fireTransformIndex]?.GetAllComponentsInChildren<Fire>().Length.ToString()
                    + "]");

                return;
            }

            Fire fire = roomFires[room]?.spawnNodes[fireTransformIndex]?.GetAllComponentsInChildren<Fire>()?[fireIndex];
            if (fire == null)
            {
                Log.Warn("[Cyclops.DouseFire could not pull Fire object at index: " + fireIndex.ToString()
                    + " Fire Index Count: " + roomFires[room]?.spawnNodes[fireTransformIndex]?.GetAllComponentsInChildren<Fire>().Length.ToString()
                    + "]");

                return;
            }

            fire.Douse(douseAmount);
        }
    }
}
