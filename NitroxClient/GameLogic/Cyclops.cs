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

        /// <summary>
        /// Broadcast the damage info so the other clients know what damage sounds to make. Sends a <see cref="CyclopsDamage"/> packet
        /// </summary>
        public void OnTakeDamage(SubRoot subRoot, Optional<DamageInfo> info)
        {
            BroadcastDamageState(subRoot, info);
        }

        /// <summary>
        /// Triggers a <see cref="CyclopsDamage"/> packet
        /// </summary>
        public void OnCreatePoint(SubRoot subRoot)
        {
            BroadcastDamageState(subRoot, Optional<DamageInfo>.Empty());
        }

        /// <summary>
        /// Called when the player repairs a <see cref="CyclopsDamagePoint"/>. Right now it's not possible to partially repair because it would be difficult to implement.
        /// <see cref="CyclopsDamagePoint"/>s are coupled with <see cref="LiveMixin"/>, which is used with just about anything that has health.
        /// I would need to hook onto <see cref="LiveMixin.AddHealth(float)"/>, or maybe the repair gun event to catch when something repairs a damage point, which I don't 
        /// believe is worth the effort. A <see cref="CyclopsDamagePoint"/> is already fully repaired in a little over a second. This can trigger sending 
        /// <see cref="CyclopsDamagePointHealthChanged"/> and <see cref="CyclopsDamage"/> packets
        /// </summary>
        public void OnDamagePointRepaired(SubRoot subRoot, CyclopsDamagePoint damagePoint, float repairAmount)
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
        /// Triggers a <see cref="CyclopsDamage"/> packet
        /// </summary>
        public void OnCreateFire(SubRoot subRoot, SubFire.RoomFire startInRoom)
        {
            BroadcastDamageState(subRoot, Optional<DamageInfo>.Empty());
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
        /// Send out a <see cref="CyclopsDamage"/> packet
        /// </summary>
        private void BroadcastDamageState(SubRoot subRoot, Optional<DamageInfo> info)
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
        private IEnumerable<int> GetActiveDamagePoints(SubRoot subRoot)
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
        /// Get all of the index locations of all the fires on the <see cref="SubRoot"/>. <see cref="SubFire.RoomFire.spawnNodes"/> contains
        /// a static list of all possible fire nodes.
        /// </summary>
        private IEnumerable<SerializableRoomFire> GetActiveRoomFires(SubFire subFire)
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

            // subFire.ReflectionSet("roomFires", roomFires);
        }
    }
}
