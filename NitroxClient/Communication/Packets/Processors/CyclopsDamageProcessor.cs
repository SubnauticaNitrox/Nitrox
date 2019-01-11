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
    /// <summary>
    /// <para>
    /// The Cyclops damage logic is handled in a lot of spots. The Cyclops has its damage managed via 2 managers and the <see cref="SubRoot"/>, all of which modify the health of the ship.
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
    /// </para>
    /// <para>
    /// Clients only receive this packet. Owners will broadcast this packet whenever it takes damage, or it receives a <see cref="CyclopsFireHealthChanged"/> 
    /// or <see cref="CyclopsDamagePointHealthChanged"/> packet from a client. The absolute health state of the Cyclops is maintained solely by the owner, but any client can participate 
    /// in repairing the Cyclops.
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

            SetActiveDamagePoints(cyclops, packet.DamagePointIndexes,
                packet.SubHealth,
                packet.DamageManagerHealth,
                packet.SubFireHealth);

            SetActiveRoomFires(cyclops, packet.RoomFires,
                packet.SubHealth,
                packet.DamageManagerHealth,
                packet.SubFireHealth);
        }

        /// <summary>
        /// Add/remove <see cref="CyclopsDamagePoint"/>s until it matches the <paramref name="damagePointIndexes"/> array passed.
        /// </summary>
        private void SetActiveDamagePoints(SubRoot cyclops, int[] damagePointIndexes, float subHealth, float damageManagerHealth, float subFireHealth)
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

                            Log.Debug("[CyclopsDamageProcessor Creating DamagePoint index: " + damagePointsIndex.ToString()
                                + " all packet DamagePoint Indexes: " + string.Join(", ", damagePointIndexes.Select(x => x.ToString()).ToArray()) + "]");
                        }

                        packetDamagePointsIndex++;
                    }
                    else
                    {
                        // If it's active, but not in the list, it must have been repaired.
                        if (damageManager.damagePoints[damagePointsIndex].gameObject.activeSelf)
                        {
                            Log.Debug("[CyclopsDamageProcessor Repairing DamagePoint index: " + damagePointsIndex.ToString()
                                + " all packet DamagePoint Indexes: " + string.Join(", ", damagePointIndexes.Select(x => x.ToString()).ToArray()) + "]");

                            RepairDamagePoint(damageManager, damagePointsIndex);
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
                Log.Debug("[CyclopsDamageProcessor No DamagePoints in packet, repairing all DamagePoints]");

                // None should be active.
                for (int i = 0; i < damageManager.damagePoints.Length; i++)
                {
                    if (damageManager.damagePoints[i].gameObject.activeSelf)
                    {
                        RepairDamagePoint(damageManager, i);
                    }
                }
            }

            cyclops.gameObject.RequireComponent<LiveMixin>().health = subHealth;
            damageManager.subLiveMixin.health = damageManagerHealth;
            cyclops.gameObject.RequireComponent<SubFire>().liveMixin.health = subFireHealth;

            // unusedDamagePoints is checked against damagePoints to determine if there's enough damage points. Failing to set the new list
            // of unusedDamagePoints will cause random DamagePoints to appear.
            damageManager.ReflectionSet("unusedDamagePoints", unusedDamagePoints);
            // Visual update only to show the water leaking through the window and various hull points based on missing health.
            damageManager.ReflectionCall("ToggleLeakPointsBasedOnDamage", false, false, null);
        }

        /// <summary>
        /// Add/remove fires until it matches the <paramref name="roomFires"/> array.
        /// </summary>
        private void SetActiveRoomFires(SubRoot subRoot, SerializableRoomFire[] roomFires, float subHealth, float damageManagerHealth, float subFireHealth)
        {
            SubFire subFire = subRoot.gameObject.RequireComponent<SubFire>();
            Dictionary<CyclopsRooms, SubFire.RoomFire> roomFiresDict = (Dictionary<CyclopsRooms, SubFire.RoomFire>)subFire.ReflectionGet("roomFires");

            // We can do a look for fires that shouldn't be around by looping through GetAllFires.
            foreach (KeyValuePair<CyclopsRooms, SubFire.RoomFire> keyValuePair in roomFiresDict)
            {
                if (roomFires.Any(x => x.Room == keyValuePair.Key))
                {
                    SerializableRoomFire roomFire = roomFires.FirstOrDefault(x => x.Room == keyValuePair.Key);

                    Log.Info("[Cyclops.SetActiveRoomFires"
                        + " Room matched: " + roomFire.Room.ToString()
                        + " FireNodeIndexes: " + string.Join(", ", roomFire.ActiveRoomFireNodes.Select(x => x.ToString()).ToArray()) + "]");

                    // There's fires that are supposed to be here.
                    for (int nodesIndex = 0; nodesIndex < keyValuePair.Value.spawnNodes.Length; nodesIndex++)
                    {
                        if (roomFire.ActiveRoomFireNodes.Any(x => x.NodeIndex == nodesIndex))
                        {
                            int fireCount = roomFire.ActiveRoomFireNodes.FirstOrDefault(x => x.NodeIndex == nodesIndex).FireCount;

                            Log.Info("[Cyclops.SetActiveRoomFires"
                                + " NodeIndexes match: " + nodesIndex.ToString()
                                + " Expected Children: " + keyValuePair.Value.spawnNodes[nodesIndex].childCount.ToString()
                                + " Actual Children: " + fireCount.ToString() + "]");

                            if (keyValuePair.Value.spawnNodes[nodesIndex].childCount < fireCount)
                            {
                                // A while statement was locking the game. I'm figuring the process of creating a fire does not happen
                                // on the same thread.
                                int fireCountToAdd = keyValuePair.Value.spawnNodes[nodesIndex].childCount - fireCount;

                                for (int i = 0; i < fireCountToAdd; i++)
                                {
                                    Log.Info("[Cyclops.SetActiveRoomFires"
                                    + " Creating Fire number: " + i.ToString()
                                    + " Expected fires: " + fireCountToAdd.ToString() + "]");

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
                                        Log.Info("[Cyclops.SetActiveRoomFires"
                                        + " Extinguishing Fire number: " + (keyValuePair.Value.spawnNodes[nodesIndex].childCount < 1).ToString()
                                        + " Expected fires: " + fireCount.ToString() + "]");

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
                    Log.Info("[Cyclops.SetActiveRoomFires No fires found in room:" + keyValuePair.Key + ". Extinguishing all fires]");

                    // Remove all of the fires in here, they shouldn't exist.
                    for (int nodeIndex = 0; nodeIndex < keyValuePair.Value.spawnNodes.Length; nodeIndex++)
                    {
                        if (keyValuePair.Value.spawnNodes[nodeIndex].childCount > 0)
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

            // Update the health of the Cyclops
            subRoot.GetComponent<LiveMixin>().health = subHealth;
            subRoot.GetComponent<CyclopsExternalDamageManager>().subLiveMixin.health = damageManagerHealth;
            subFire.liveMixin.health = subFireHealth;

            subFire.ReflectionSet("roomFires", roomFiresDict);
        }

        /// <summary>
        /// Completely repairs a damage point regardless of health.
        /// </summary>
        /// <param name="damageManager">The <see cref="CyclopsExternalDamageManager"/> located in <see cref="SubRoot.damageManager"/></param>
        /// <param name="damagePointsIndex">The index of the <see cref="CyclopsDamagePoint"/></param>
        private void RepairDamagePoint(CyclopsExternalDamageManager damageManager, int damagePointsIndex)
        {
            using (packetSender.Suppress<CyclopsDamage>())
            {
                using (packetSender.Suppress<CyclopsDamagePointHealthChanged>())
                {
                    damageManager.damagePoints[damagePointsIndex].OnRepair();
                    // damageManager.RepairPoint(damageManager.damagePoints[damagePointsIndex]);
                }
            }
        }

        /// <summary>
        /// Create a new fire in a specific room and node. Calling <see cref="SubFire.CreateFire(SubFire.RoomFire)"/> would cause it to use 
        /// a random number generator to choose the node.
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
        /// Executes the logic in <see cref="Fire.Douse(float)"/> while not calling the method itself to avoid endless looped calls. 
        /// If the fire is extinguished (no health left), it will pass a large float to trigger the private <see cref="Fire.Extinguish()"/> method.
        /// </summary>
        /// <param name="room">The room the fire is in. Generally passed via <see cref="CyclopsDamage"/> packet.</param>
        /// <param name="fire">The fire to douse. Generally passed via <see cref="CyclopsDamage"/> packet. Passing a large float will extinguish the fire.</param>
        public void DouseFire(SubFire fireManager, CyclopsRooms room, int fireTransformIndex, int fireIndex, float douseAmount)
        {
            Dictionary<CyclopsRooms, SubFire.RoomFire> roomFires = (Dictionary<CyclopsRooms, SubFire.RoomFire>)fireManager.ReflectionGet("roomFires");

            // What a beautiful monster this is.
            if (roomFires[room]?.spawnNodes[fireTransformIndex]?.GetAllComponentsInChildren<Fire>()?.Length <= fireIndex)
            {
                Log.Error("[Cyclops.DouseFire fireIndex larger than number of Fires fireIndex: " + fireIndex.ToString()
                    + " Fire Count: " + roomFires[room]?.spawnNodes[fireTransformIndex]?.GetAllComponentsInChildren<Fire>().Length.ToString()
                    + "]");

                return;
            }

            Fire fire = roomFires[room]?.spawnNodes[fireTransformIndex]?.GetAllComponentsInChildren<Fire>()?[fireIndex];
            if (fire == null)
            {
                Log.Error("[Cyclops.DouseFire could not pull Fire object at index: " + fireIndex.ToString()
                    + " Fire Index Count: " + roomFires[room]?.spawnNodes[fireTransformIndex]?.GetAllComponentsInChildren<Fire>().Length.ToString()
                    + "]");

                return;
            }

            using (packetSender.Suppress<CyclopsFireHealthChanged>())
            {
                fire.Douse(douseAmount);
            }
        }
    }
}
