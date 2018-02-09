using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NitroxClient.Communication;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Cyclops
    {
        private readonly IPacketSender packetSender;

        public Cyclops(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
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

        /// <summary>
        /// Spawn a new Cyclops. <paramref name="subRoot"/> will not have its position or rotation declared upon spawning. You must pull those values from
        /// elsewhere.
        /// </summary>
        public void SpawnNew(GameObject subRoot, Vector3 position, Quaternion rotation)
        {
            string guid = GuidHelper.GetGuid(subRoot.gameObject);
            VehicleMovement packet = new VehicleMovement(packetSender.PlayerId, position, Vector3.zero, rotation, Vector3.zero, TechType.Cyclops, guid, 0, 0, false);
            packetSender.Send(packet);
        }

        /// <summary>
        /// After the damage is calculated and all of the <see cref="CyclopsDamagePoint"/>s and <see cref="Fire"/>s have been set, it's packaged and sent out to the server.
        /// </summary>
        public void OnTakeDamage(SubRoot subRoot, DamageInfo info)
        {
            string subGuid = GuidHelper.GetGuid(subRoot.gameObject);
            LiveMixin subHealth = subRoot.gameObject.RequireComponent<LiveMixin>();

            SerializableDamageInfo damageInfo = null;

            if (info != null)
            {
                // Source of the damage. Used if the damage done to the Cyclops was not calculated on other clients. Currently it's just used to figure out what sounds and
                // visual effects should be used.
                SerializableDamageInfo serializedDamageInfo = new SerializableDamageInfo()
                {
                    OriginalDamage = info.originalDamage,
                    Damage = info.damage,
                    Position = info.position,
                    Type = info.type,
                    DealerGuid = info.dealer != null ? GuidHelper.GetGuid(info.dealer) : string.Empty
                };
            }

            if (subHealth.health > 0)
            {
                int[] damagePointIndexes = GetActiveDamagePoints(subRoot).ToArray();
                SerializableRoomFire[] firePointIndexes = GetActiveRoomFires(subRoot.GetComponent<SubFire>()).ToArray();

                CyclopsDamage packet = new CyclopsDamage(subGuid, subRoot.GetComponent<LiveMixin>().health, subRoot.damageManager.subLiveMixin.health, subRoot.GetComponent<SubFire>().liveMixin.health, damagePointIndexes, firePointIndexes, damageInfo);
                packetSender.Send(packet);
            }
            else
            {
                // Only a client side damage event can trigger this.
                Log.Debug("[CyclopsDestroyedProcessor Guid: " + subGuid + " received OnTakeDamage after health is 0, packet sent.]");

                // He ded. They only need to know it's going to blow up.
                CyclopsDestroyed packet = new CyclopsDestroyed(subGuid);
                packetSender.Send(packet);
            }
        }

        /// <summary>
        /// Called externally when the player repairs a <see cref="CyclopsDamagePoint"/>.
        /// </summary>
        public void OnDamagePointRepaired(SubRoot subRoot)
        {
            // No way to currently communicate just one point was affected. They're passed all at once, so we just trigger that to happen.
            OnTakeDamage(subRoot, null);
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
        /// Add/remove <see cref="CyclopsDamagePoint"/>s until it matches the <paramref name="damagePointIndexes"/> array passed.
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
                        // Must be an added damage point. There's no method to activate a specific damage point, so we have to do it manually.
                        if (!damageManager.damagePoints[damagePointsIndex].gameObject.activeSelf)
                        {
                            // Copied from CyclopsExternalDamageManager.CreatePoint(), except without the random index.
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

                            RepairDamagePoint(damageManager, unusedDamagePoints, damagePointsIndex);
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
                        RepairDamagePoint(damageManager, unusedDamagePoints, i);
                    }
                }
            }

            // Update the health of the Cyclops
            cyclops.gameObject.RequireComponent<LiveMixin>().health = subHealth;
            damageManager.subLiveMixin.health = damageManagerHealth;
            cyclops.gameObject.RequireComponent<SubFire>().liveMixin.health = subFireHealth;

            // unusedDamagePoints is checked against damagePoints to determine if there's enough damage points. Failing to set the new list
            // of unusedDamagePoints will cause random DamagePoints to appear.
            damageManager.ReflectionSet("unusedDamagePoints", unusedDamagePoints);
            // ToggleLeakPointsBasedOnDamage is a visual update only.
            damageManager.ReflectionCall("ToggleLeakPointsBasedOnDamage", false, false, null);
        }

        /// <summary>
        /// <para>
        /// At first I thought "Oh look, they made it easy for me to figure out where the fire is by referencing the <see cref="CyclopsRooms"/> enum!"
        /// Nope, it's just a reference to a list of <see cref="Transform"/> that are randomly chosen and activated at <see cref="SubFire.CreateFire(SubFire.RoomFire)"/>
        /// , which then creates/activates a <see cref="Fire"/> set as a child of the <see cref="Transform"/>. 
        /// </para>
        /// <para>
        /// The only saving grace is that <see cref="Fire"/> does not spawn other <see cref="Fire"/>s when it "grows", like I feared it would. 
        /// Instead, it scales the transform of the fire up as its <see cref="Fire.livemixin.health"/> increases. The added fire effects are the responsibility of 
        /// <see cref="Fire.fireFX"/>, not the <see cref="Fire"/> itself.
        /// </para>
        /// <para>
        /// I am entirely convinced every single aspect of the Cyclops has a random number generator attached to it.
        /// </para>
        /// </summary>
        public void OnFireCreated(GameObject subFire, DamageInfo damageInfo, CyclopsRooms room)
        {
            // Currently unused, but will be implemented when ownership is implemented. At this moment, the free-for-all nature of damage addition/removal
            // means any fire change at all triggers a complete update, except when a fire is being doused by the player.
            Log.Debug("SubFire.OnTakeDamage called Cyclops.FireCreated(): " + room.ToString());
        }

        /// <summary>
        /// Create a new fire in a specific room and node. The comment at <see cref="OnFireCreated(GameObject, DamageInfo, CyclopsRooms)"/> gives an explanation
        /// as to how the fires are stored.
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
        /// Called when <see cref="Fire.Douse(float)"/> is called.
        /// </summary>
        public void OnFireDoused(Fire fire, SubRoot subRoot, float douseAmount)
        {
            SubFire subFire = subRoot.gameObject.RequireComponent<SubFire>();
            Dictionary<CyclopsRooms, SubFire.RoomFire> roomFiresDict = (Dictionary<CyclopsRooms, SubFire.RoomFire>)subFire.ReflectionGet("roomFires");

            // Crawl up the items to get the indexes we need. This is the node.
            Transform fireNode = fire.gameObject.RequireComponentInParent<Transform>();
            string fireNodeGuid = GuidHelper.GetGuid(fireNode.gameObject);
            foreach (KeyValuePair<CyclopsRooms, SubFire.RoomFire> kvp in roomFiresDict)
            {
                for (int i = 0; i < kvp.Value.spawnNodes.Length; i++)
                {
                    for (int j = 0; j < kvp.Value.spawnNodes[i].GetAllComponentsInChildren<Fire>().Length; j++)
                    {
                        if (kvp.Value.spawnNodes[i].GetAllComponentsInChildren<Fire>()[j] == fire)
                        {
                            CyclopsFireHealthChanged patch = new CyclopsFireHealthChanged(GuidHelper.GetGuid(subRoot.gameObject), kvp.Key, i, j, douseAmount);
                            packetSender.Send(patch);

                            Log.Debug("[Cyclops.OnFireDoused finished pulling values."
                                + "RoomFire: " + kvp.Key.ToString()
                                + "FireNodeIndex: " + i.ToString()
                                + "FireIndex: " + j.ToString()
                                + "]");
                        }
                    }
                }
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

            // Copied from Fire.Douse(float amount)
            float healthFraction = fire.livemixin.GetHealthFraction();
            int fmodIndexFireHealth = (int)fire.ReflectionGet("fmodIndexFireHealth");
            if (fmodIndexFireHealth < 0)
            {
                fire.ReflectionSet("fmodIndexFireHealth", fire.fireSound.GetParameterIndex("fire_health"));
            }
            fire.fireSound.SetParameterValue((int)fire.ReflectionGet("fmodIndexFireHealth"), healthFraction);
            fire.ReflectionSet("lastTimeDoused", Time.time);
            fire.livemixin.health = Mathf.Max(fire.livemixin.health - douseAmount, 0f);
            if (fire.fireFX)
            {
                fire.fireFX.amount = healthFraction;
            }
            fire.gameObject.RequireComponent<Component>().transform.localScale = Vector3.Lerp(fire.minScale, Vector3.one, healthFraction);
            if (!fire.livemixin.IsAlive() && !(bool)fire.ReflectionGet("isExtinguished"))
            {
                fire.ReflectionCall("Extinguished");
            }
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
                if (roomFires.Any(x => x.Room == keyValuePair.Key))
                {
                    SerializableRoomFire roomFire = roomFires.FirstOrDefault(x => x.Room == keyValuePair.Key);

                    Log.Debug("[Cyclops.SetActiveRoomFires"
                        + " Room matched: " + roomFire.Room.ToString()
                        + " FireNodeIndexes: " + string.Join(", ", roomFire.ActiveRoomFireNodes.Select(x => x.ToString()).ToArray()) + "]");

                    // There's fires that are supposed to be here.
                    for (int nodesIndex = 0; nodesIndex < keyValuePair.Value.spawnNodes.Length; nodesIndex++)
                    {
                        if (roomFire.ActiveRoomFireNodes.Any(x => x.NodeIndex == nodesIndex))
                        {
                            int fireCount = roomFire.ActiveRoomFireNodes.FirstOrDefault(x => x.NodeIndex == nodesIndex).FireCount;

                            Log.Debug("[Cyclops.SetActiveRoomFires"
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
                                    Log.Debug("[Cyclops.SetActiveRoomFires"
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
                                        Log.Debug("[Cyclops.SetActiveRoomFires"
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
                    Log.Debug("[Cyclops.SetActiveRoomFires No fires found in room:" + keyValuePair.Key + ". Extinguishing all fires]");

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
        /// Implements the logic for repairing a <see cref="CyclopsDamagePoint"/>. This is used over a direct call to 
        /// <see cref="CyclopsExternalDamageManager.RepairPoint(CyclopsDamagePoint)"/> because the original function would 
        /// trigger <see cref="NitroxPatcher.Patches.CyclopsDamagePoint_OnRepair_Patch"/>
        /// </summary>
        /// <param name="damageManager">The <see cref="CyclopsExternalDamageManager"/> located in <see cref="SubRoot.damageManager"/></param>
        /// <param name="unusedDamagePoints">A private variable located in <see cref="CyclopsExternalDamageManager"/></param>
        /// <param name="damagePointsIndex">The index of the <see cref="CyclopsDamagePoint"/></param>
        public void RepairDamagePoint(CyclopsExternalDamageManager damageManager, List<CyclopsDamagePoint> unusedDamagePoints, int damagePointsIndex)
        {
            FieldInfo psField = typeof(CyclopsDamagePoint).GetField("ps", BindingFlags.NonPublic | BindingFlags.Instance);
            ParticleSystem ps = (ParticleSystem)psField.GetValue(damageManager.damagePoints[damagePointsIndex]);

            if (ps != null)
            {
                ps.transform.parent = null;
                ps.Stop();
                UnityEngine.Object.Destroy(ps.gameObject, 3f);
            }

            damageManager.damagePoints[damagePointsIndex].gameObject.SetActive(false);
            unusedDamagePoints.Add(damageManager.damagePoints[damagePointsIndex]);
        }
    }
}
