using System;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Cyclops
    {
        private readonly IPacketSender packetSender;
        private readonly SimulationOwnership simulationOwnershipManager;
        private readonly Vehicles vehicles;

        public Cyclops(IPacketSender packetSender, SimulationOwnership simulationOwnershipManager, Vehicles vehicles)
        {
            this.packetSender = packetSender;
            this.simulationOwnershipManager = simulationOwnershipManager;
            this.vehicles = vehicles;
        }

        public void BroadcastToggleInternalLight(string guid, bool isOn)
        {
            vehicles.GetVehicles<CyclopsModel>(guid).InternalLightsOn = isOn;
            CyclopsToggleInternalLighting packet = new CyclopsToggleInternalLighting(guid, isOn);
            packetSender.Send(packet);
        }

        public void BroadcastToggleFloodLights(string guid, bool isOn)
        {
            vehicles.GetVehicles<CyclopsModel>(guid).FloodLightsOn = isOn;
            CyclopsToggleFloodLights packet = new CyclopsToggleFloodLights(guid, isOn);
            packetSender.Send(packet);
        }

        public void BroadcastChangeSilentRunning(string guid, bool isOn)
        {
            vehicles.GetVehicles<CyclopsModel>(guid).SilentRunningOn = isOn;
            CyclopsChangeSilentRunning packet = new CyclopsChangeSilentRunning(guid, isOn);
            packetSender.Send(packet);
        }

        public void BroadcastChangeEngineMode(string guid, CyclopsMotorMode.CyclopsMotorModes mode)
        {
            CyclopsChangeEngineMode packet = new CyclopsChangeEngineMode(guid, mode);
            packetSender.Send(packet);
        }

        public void BroadcastToggleEngineState(string guid, bool isOn, bool isStarting)
        {
            CyclopsToggleEngineState packet = new CyclopsToggleEngineState(guid, isOn, isStarting);
            packetSender.Send(packet);
        }

        public void BroadcastActivateHorn(string guid)
        {
            CyclopsActivateHorn packet = new CyclopsActivateHorn(guid);
            packetSender.Send(packet);
        }

        public void BroadcastLaunchDecoy(string guid)
        {
            CyclopsDecoyLaunch packet = new CyclopsDecoyLaunch(guid);
            packetSender.Send(packet);
        }

        public void BroadcastChangeSonarState(string guid, bool active)
        {
            vehicles.GetVehicles<CyclopsModel>(guid).SonarOn = active;
            CyclopsChangeSonarMode packet = new CyclopsChangeSonarMode(guid,active);
            packetSender.Send(packet);
        }

        public void BroadcastSonarPing(string guid)
        {
            CyclopsSonarPing packet = new CyclopsSonarPing(guid);
            packetSender.Send(packet);
        }

        public void BroadcastChangeShieldState(string guid, bool isOn)
        {
            vehicles.GetVehicles<CyclopsModel>(guid).ShieldOn = isOn;
            CyclopsChangeShieldMode packet = new CyclopsChangeShieldMode(guid, isOn);
            packetSender.Send(packet);
        }

        public void BroadcastActivateFireSuppression(string guid)
        {
            CyclopsFireSuppression packet = new CyclopsFireSuppression(guid);
            packetSender.Send(packet);
        }

        public void SetInternalLighting(string guid, bool isOn)
        {
            GameObject cyclops = GuidHelper.RequireObjectFrom(guid);
            CyclopsLightingPanel lighting = cyclops.RequireComponentInChildren<CyclopsLightingPanel>();

            if (lighting.lightingOn != isOn)
            {
                using (packetSender.Suppress<CyclopsToggleInternalLighting>())
                {
                    lighting.ToggleInternalLighting();
                }
            }
        }

        public void SetFloodLighting(string guid, bool isOn)
        {
            GameObject cyclops = GuidHelper.RequireObjectFrom(guid);
            CyclopsLightingPanel lighting = cyclops.RequireComponentInChildren<CyclopsLightingPanel>();

            if (lighting.floodlightsOn != isOn)
            {
                using (packetSender.Suppress<CyclopsToggleFloodLights>())
                {
                    lighting.ToggleFloodlights();
                }
            }
        }

        internal void SetAllModes(string guid, CyclopsModel cyclopsModel)
        {
            
        }

        /// <summary>
        /// Triggers a <see cref="CyclopsDamage"/> packet
        /// </summary>
        public void OnCreateDamagePoint(SubRoot subRoot)
        {
            BroadcastDamageState(subRoot, Optional<DamageInfo>.Empty());
        }

        /// <summary>
        /// Called when the player repairs a <see cref="CyclopsDamagePoint"/>. Right now it's not possible to partially repair because it would be difficult to implement.
        /// <see cref="CyclopsDamagePoint"/>s are coupled with <see cref="LiveMixin"/>, which is used with just about anything that has health.
        /// I would need to hook onto <see cref="LiveMixin.AddHealth(float)"/>, or maybe the repair gun event to catch when something repairs a damage point, which I don't 
        /// believe is worth the effort. A <see cref="CyclopsDamagePoint"/> is already fully repaired in a little over a second. This can trigger sending 
        /// <see cref="CyclopsDamagePointRepaired"/> and <see cref="CyclopsDamage"/> packets
        /// </summary>
        public void OnDamagePointRepaired(SubRoot subRoot, CyclopsDamagePoint damagePoint, float repairAmount)
        {
            string subGuid = GuidHelper.GetGuid(subRoot.gameObject);

            for (int i = 0; i < subRoot.damageManager.damagePoints.Length; i++)
            {
                if (subRoot.damageManager.damagePoints[i] == damagePoint)
                {
                    CyclopsDamagePointRepaired packet = new CyclopsDamagePointRepaired(subGuid, i, repairAmount);
                    packetSender.Send(packet);

                    return;
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
                CyclopsDamageInfoData damageInfo = null;

                if (info.IsPresent())
                {
                    DamageInfo damage = info.Get();
                    // Source of the damage. Used if the damage done to the Cyclops was not calculated on other clients. Currently it's just used to figure out what sounds and
                    // visual effects should be used.
                    CyclopsDamageInfoData serializedDamageInfo = new CyclopsDamageInfoData(subGuid,
                        damage.dealer != null ? GuidHelper.GetGuid(damage.dealer) : string.Empty,
                        damage.originalDamage,
                        damage.damage,
                        damage.position,
                        damage.type);
                }

                int[] damagePointIndexes = GetActiveDamagePoints(subRoot).ToArray();
                CyclopsFireData[] firePoints = GetActiveRoomFires(subRoot.GetComponent<SubFire>()).ToArray();

                CyclopsDamage packet = new CyclopsDamage(subGuid, subRoot.GetComponent<LiveMixin>().health, subRoot.damageManager.subLiveMixin.health, subRoot.GetComponent<SubFire>().liveMixin.health, damagePointIndexes, firePoints, damageInfo);
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
        private IEnumerable<CyclopsFireData> GetActiveRoomFires(SubFire subFire)
        {
            string subRootGuid = GuidHelper.GetGuid(subFire.subRoot.gameObject);
            Dictionary<CyclopsRooms, SubFire.RoomFire> roomFires = (Dictionary<CyclopsRooms, SubFire.RoomFire>)subFire.ReflectionGet("roomFires");

            foreach (KeyValuePair<CyclopsRooms, SubFire.RoomFire> roomFire in roomFires)
            {
                for (int i = 0; i < roomFire.Value.spawnNodes.Length; i++)
                {
                    if (roomFire.Value.spawnNodes[i].childCount > 0)
                    {
                        yield return new CyclopsFireData(GuidHelper.GetGuid(roomFire.Value.spawnNodes[i].GetComponentInChildren<Fire>().gameObject), 
                            subRootGuid,
                            roomFire.Key,
                            i);
                    }
                }
            }
        }

        
    }
}
