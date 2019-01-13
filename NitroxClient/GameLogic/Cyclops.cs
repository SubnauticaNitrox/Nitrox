using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Cyclops
    {
        private readonly IPacketSender packetSender;
        private readonly SimulationOwnership simulationOwnershipManager;

        /// <summary>
        /// KeyValuePair<Guid, douseAmount>. Used in <see cref="OnFireDoused(Fire, SubRoot, float)"/> to reduce
        /// the <see cref="CyclopsFireHealthChanged"/> packet spam as fires are being doused. A packet is only sent after
        /// the douse amount surpasses <see cref="FIRE_DOUSE_AMOUNT_TRIGGER"/>
        /// </summary>
        private readonly Dictionary<string, float> fireDouseAmount = new Dictionary<string, float>();
        /// <summary>
        /// Each extinguisher hit is from 0.23 to 0.45. 12 is a bit less than half a second of full extinguishing.
        /// </summary>
        private const float FIRE_DOUSE_AMOUNT_TRIGGER = 12f;

        public Cyclops(IPacketSender packetSender, SimulationOwnership simulationOwnershipManager)
        {
            this.packetSender = packetSender;
            this.simulationOwnershipManager = simulationOwnershipManager;
        }

        public void ToggleInternalLight(string guid, bool isOn)
        {
            CyclopsToggleInternalLighting packet = new CyclopsToggleInternalLighting(guid, isOn);
            packetSender.Send(packet);
        }

        public void ToggleFloodLights(string guid, bool isOn)
        {
            CyclopsToggleFloodLights packet = new CyclopsToggleFloodLights(guid, isOn);
            packetSender.Send(packet);
        }

        public void BeginSilentRunning(string guid)
        {
            CyclopsBeginSilentRunning packet = new CyclopsBeginSilentRunning(guid);
            packetSender.Send(packet);
        }

        public void ChangeEngineMode(string guid, CyclopsMotorMode.CyclopsMotorModes mode)
        {
            CyclopsChangeEngineMode packet = new CyclopsChangeEngineMode(guid, mode);
            packetSender.Send(packet);
        }

        public void ToggleEngineState(string guid, bool isOn, bool isStarting)
        {
            CyclopsToggleEngineState packet = new CyclopsToggleEngineState(guid, isOn, isStarting);
            packetSender.Send(packet);
        }

        public void ActivateHorn(string guid)
        {
            CyclopsActivateHorn packet = new CyclopsActivateHorn(guid);
            packetSender.Send(packet);
        }

        public void ActivateShield(string guid)
        {
            CyclopsActivateShield packet = new CyclopsActivateShield(guid);
            packetSender.Send(packet);
        }

        public void OnCreatePoint(SubRoot subRoot)
        {
            BroadcastDamageChange(subRoot, Optional<DamageInfo>.Empty());
        }

        /// <summary>
        /// Called when the player repairs a <see cref="CyclopsDamagePoint"/>. Right now it's not possible to partially repair because it would be difficult to implement.
        /// <see cref="CyclopsDamagePoint"/>s are coupled with <see cref="LiveMixin"/>, which is used with just about anything that has health.
        /// I would need to hook onto <see cref="LiveMixin.AddHealth(float)"/>, or maybe the repair gun event to catch when something repairs a damage point, which I don't 
        /// believe is worth the effort. A <see cref="CyclopsDamagePoint"/> is already fully repaired in a little over a second. This can trigger sending 
        /// <see cref="CyclopsDamagePointHealthChanged"/> and <see cref="CyclopsDamage"/> packets
        /// </summary>
        public void OnDamagePointHealthChanged(SubRoot subRoot, CyclopsDamagePoint damagePoint, float repairAmount)
        {
            string subGuid = GuidHelper.GetGuid(subRoot.gameObject);

            for (int i = 0; i < subRoot.damageManager.damagePoints.Length; i++)
            {
                if (subRoot.damageManager.damagePoints[i] == damagePoint)
                {
                    CyclopsDamagePointHealthChanged packet = new CyclopsDamagePointHealthChanged(subGuid, i, repairAmount);
                    packetSender.Send(packet);

                    return;
                }
            }
        }

        /// <summary>
        /// Set the health of a <see cref="CyclopsDamagePoint"/>. This can trigger sending <see cref="CyclopsDamagePointHealthChanged"/> packets
        /// </summary>
        /// <param name="repairAmount">The max health of the point is 1. 999 is passed to trigger a full repair of the <see cref="CyclopsDamagePoint"/></param>
        public void SetDamagePointHealth(SubRoot subRoot, int damagePointIndex, float repairAmount)
        {
            subRoot.damageManager.damagePoints[damagePointIndex].liveMixin.AddHealth(repairAmount);
        }

        /// <summary>
        /// Send out a <see cref="CyclopsDamage"/> packet
        /// </summary>
        private void BroadcastDamageChange(SubRoot subRoot, Optional<DamageInfo> info)
        {
            string subGuid = GuidHelper.GetGuid(subRoot.gameObject);
            LiveMixin subHealth = subRoot.gameObject.RequireComponent<LiveMixin>();
            
            if (subHealth.health > 0)
            {
                SerializableDamageInfo damageInfo = null;

                if (info.IsPresent())
                {
                    DamageInfo damage = info.Get();
                    // Source of the damage. Used if the damage done to the Cyclops was not calculated on other clients. Currently it's just used to figure out what sounds and
                    // visual effects should be used.
                    SerializableDamageInfo serializedDamageInfo = new SerializableDamageInfo()
                    {
                        OriginalDamage = damage.originalDamage,
                        Damage = damage.damage,
                        Position = damage.position,
                        Type = damage.type,
                        DealerGuid = damage.dealer != null ? GuidHelper.GetGuid(damage.dealer) : string.Empty
                    };
                }

                int[] damagePointIndexes = GetActiveDamagePoints(subRoot).ToArray();
                SerializableRoomFire[] firePointIndexes = GetActiveRoomFires(subRoot.GetComponent<SubFire>()).ToArray();

                CyclopsDamage packet = new CyclopsDamage(subGuid, subRoot.GetComponent<LiveMixin>().health, subRoot.damageManager.subLiveMixin.health, subRoot.GetComponent<SubFire>().liveMixin.health, damagePointIndexes, firePointIndexes, damageInfo);
                packetSender.Send(packet);
            }
            else
            {
                // RIP
                CyclopsDestroyed packet = new CyclopsDestroyed(subGuid);
                packetSender.Send(packet);
            }
        }

        /// <summary>
        /// Get all of the index locations of <see cref="CyclopsDamagePoint"/>s in <see cref="CyclopsExternalDamageManager.damagePoints"/>.
        /// </summary>
        public IEnumerable<int> GetActiveDamagePoints(SubRoot subRoot)
        {
            for (int i = 0; i < subRoot.damageManager.damagePoints.Length; i++)
            {
                if (subRoot.damageManager.damagePoints[i].gameObject.activeSelf)
                {
                    yield return i;
                }
            }
        }

        /// <summary>
        /// Add/remove <see cref="CyclopsDamagePoint"/>s until it matches the <paramref name="damagePointIndexes"/> array passed
        /// </summary>
        public void SetActiveDamagePoints(SubRoot cyclops, int[] damagePointIndexes, float subHealth, float damageManagerHealth, float subFireHealth)
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
                            SetDamagePointHealth(cyclops, damagePointsIndex, 999);
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
                        SetDamagePointHealth(cyclops, i, 999);
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

        public void OnCreateFire(SubRoot subRoot, SubFire.RoomFire startInRoom)
        {
            BroadcastDamageChange(subRoot, Optional<DamageInfo>.Empty());
        }

        /// <summary>
        /// Create a new fire in a specific room and node. Calling <see cref="SubFire.CreateFire(SubFire.RoomFire)"/> would just cause it to use 
        /// a random number generator to choose the node. This can trigger sending <see cref="CyclopsDamage"/> packets
        /// </summary>
        public void CreateFire(SubRoot subRoot, KeyValuePair<CyclopsRooms, SubFire.RoomFire> fireRoom, int nodeIndex)
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
        /// Called when <see cref="Fire.Douse(float)"/> is called. This can trigger sending <see cref="CyclopsFireHealthChanged"/> packets
        /// </summary>
        public void OnFireDoused(Fire fire, SubRoot subRoot, float douseAmount)
        {
            SubFire subFire = subRoot.gameObject.RequireComponent<SubFire>();
            Dictionary<CyclopsRooms, SubFire.RoomFire> roomFiresDict = (Dictionary<CyclopsRooms, SubFire.RoomFire>)subFire.ReflectionGet("roomFires");
            string fireGuid = GuidHelper.GetGuid(fire.gameObject);

            if (!fireDouseAmount.ContainsKey(fireGuid))
            {
                fireDouseAmount.Add(fireGuid, douseAmount);
            }
            else
            {
                float summedDouseAmount = fireDouseAmount[fireGuid] + douseAmount;

                if (summedDouseAmount > FIRE_DOUSE_AMOUNT_TRIGGER)
                {
                    // It is significantly faster to keep the key as a 0 value than to remove it and re-add it later.
                    fireDouseAmount[fireGuid] = 0;

                    // Crawl up the items to get the indexes we need. This is the node.
                    Transform fireNode = fire.gameObject.RequireComponentInParent<Transform>();
                    foreach (KeyValuePair<CyclopsRooms, SubFire.RoomFire> kvp in roomFiresDict)
                    {
                        for (int i = 0; i < kvp.Value.spawnNodes.Length; i++)
                        {
                            Fire[] fires = kvp.Value.spawnNodes[i].GetAllComponentsInChildren<Fire>();

                            // The actual fires. Fires look to be procedurally generated, so a blind index lookup isn't possible.
                            for (int j = 0; j < fires.Length; j++)
                            {
                                if (fires[j] == fire)
                                {
                                    CyclopsFireHealthChanged patch = new CyclopsFireHealthChanged(
                                        GuidHelper.GetGuid(subRoot.gameObject),
                                        kvp.Key,
                                        i,
                                        j,
                                        summedDouseAmount);

                                    packetSender.Send(patch);

                                    return;
                                }
                            }
                        }
                    }

                    Log.Warn("[Cyclops.OnFireDoused could not locate fire!"
                        + " Sub Guid: " + GuidHelper.GetGuid(subRoot.gameObject)
                        + " Fire Guid: " + fireGuid
                        + "]");
                }
                else
                {
                    fireDouseAmount[fireGuid] = summedDouseAmount;
                }
            }
        }

        /// <summary>
        /// Executes the logic in <see cref="Fire.Douse(float)"/> while not calling the method itself to avoid endless looped calls. 
        /// If the fire is extinguished (no health left), it will pass a large float to trigger the private <see cref="Fire.Extinguish()"/> method.
        /// This can trigger sending <see cref="CyclopsFireHealthChanged"/> and <see cref="CyclopsDamage"/> packets
        /// </summary>
        public void DouseFire(SubFire fireManager, CyclopsRooms room, int fireTransformIndex, int fireIndex, float douseAmount)
        {
            Dictionary <CyclopsRooms, SubFire.RoomFire> roomFires = (Dictionary<CyclopsRooms, SubFire.RoomFire>)fireManager.ReflectionGet("roomFires");

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

        /// <summary>
        /// Get all of the index locations of all the fires on the <see cref="SubRoot"/>. <see cref="SubFire.RoomFire.spawnNodes"/> contains
        /// a static list of all possible fire nodes.
        /// </summary>
        public IEnumerable<SerializableRoomFire> GetActiveRoomFires(SubFire subFire)
        {
            string subRootGuid = GuidHelper.GetGuid(subFire.subRoot.gameObject);
            Dictionary<CyclopsRooms, SubFire.RoomFire> roomFires = (Dictionary<CyclopsRooms, SubFire.RoomFire>)subFire.ReflectionGet("roomFires");

            foreach (KeyValuePair<CyclopsRooms, SubFire.RoomFire> roomFire in roomFires)
            {
                // There be fires here.
                if (roomFire.Value.fireValue > 0)
                {
                    List<SerializableFireNode> activeNodes = new List<SerializableFireNode>();
                    for (int i = 0; i < roomFire.Value.spawnNodes.Length; i++)
                    {
                        // Is this a fire? Copied from SubFire.CreateFire(SubFire.RoomFire startInRoom)
                        if (roomFire.Value.spawnNodes[i].childCount > 0)
                        {
                            activeNodes.Add(new SerializableFireNode()
                            {
                                NodeIndex = i,
                                FireCount = roomFire.Value.spawnNodes[i].childCount
                            });
                        }
                    }

                    SerializableRoomFire newRoomFire = new SerializableRoomFire()
                    {
                        Room = roomFire.Key,
                        ActiveRoomFireNodes = activeNodes.ToArray()
                    };

                    yield return newRoomFire;
                }
            }

            subFire.ReflectionSet("roomFires", roomFires);
        }

        /// <summary>
        /// Add/remove fires until it matches the <paramref name="roomFires"/> array.
        /// </summary>
        public void SetActiveRoomFires(SubRoot subRoot, SerializableRoomFire[] roomFires, float subHealth, float damageManagerHealth, float subFireHealth)
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

            // Update the health of the Cyclops
            subRoot.GetComponent<LiveMixin>().health = subHealth;
            subRoot.GetComponent<CyclopsExternalDamageManager>().subLiveMixin.health = damageManagerHealth;
            subFire.liveMixin.health = subFireHealth;

            subFire.ReflectionSet("roomFires", roomFiresDict);
        }
    }
}
