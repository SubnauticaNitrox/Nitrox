using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
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

        public void BroadcastToggleInternalLight(NitroxId id, bool isOn)
        {
            vehicles.GetVehicles<CyclopsModel>(id).InternalLightsOn = isOn;
            CyclopsToggleInternalLighting packet = new CyclopsToggleInternalLighting(id, isOn);
            packetSender.Send(packet);
        }

        public void BroadcastToggleFloodLights(NitroxId id, bool isOn)
        {
            vehicles.GetVehicles<CyclopsModel>(id).FloodLightsOn = isOn;
            CyclopsToggleFloodLights packet = new CyclopsToggleFloodLights(id, isOn);
            packetSender.Send(packet);
        }

        public void BroadcastChangeSilentRunning(NitroxId id, bool isOn)
        {
            vehicles.GetVehicles<CyclopsModel>(id).SilentRunningOn = isOn;
            CyclopsChangeSilentRunning packet = new CyclopsChangeSilentRunning(id, isOn);
            packetSender.Send(packet);
        }

        public void BroadcastChangeEngineMode(NitroxId id, CyclopsMotorMode.CyclopsMotorModes mode)
        {
            vehicles.GetVehicles<CyclopsModel>(id).EngineMode = mode;
            CyclopsChangeEngineMode packet = new CyclopsChangeEngineMode(id, mode);
            packetSender.Send(packet);
        }

        public void BroadcastToggleEngineState(NitroxId id, bool isOn, bool isStarting)
        {
            vehicles.GetVehicles<CyclopsModel>(id).EngineState = isStarting;
            CyclopsToggleEngineState packet = new CyclopsToggleEngineState(id, isOn, isStarting);
            packetSender.Send(packet);
        }

        public void BroadcastActivateHorn(NitroxId id)
        {
            CyclopsActivateHorn packet = new CyclopsActivateHorn(id);
            packetSender.Send(packet);
        }

        public void BroadcastLaunchDecoy(NitroxId id)
        {
            CyclopsDecoyLaunch packet = new CyclopsDecoyLaunch(id);
            packetSender.Send(packet);
        }

        public void BroadcastChangeSonarState(NitroxId id, bool active)
        {
            vehicles.GetVehicles<CyclopsModel>(id).SonarOn = active;
            CyclopsChangeSonarMode packet = new CyclopsChangeSonarMode(id, active);
            packetSender.Send(packet);
        }

        public void BroadcastSonarPing(NitroxId id)
        {
            CyclopsSonarPing packet = new CyclopsSonarPing(id);
            packetSender.Send(packet);
        }

        public void BroadcastChangeShieldState(NitroxId id, bool isOn)
        {
            vehicles.GetVehicles<CyclopsModel>(id).ShieldOn = isOn;
            CyclopsChangeShieldMode packet = new CyclopsChangeShieldMode(id, isOn);
            packetSender.Send(packet);
        }

        public void BroadcastActivateFireSuppression(NitroxId id)
        {
            CyclopsFireSuppression packet = new CyclopsFireSuppression(id);
            packetSender.Send(packet);
        }

        public void SetInternalLighting(NitroxId id, bool isOn)
        {
            GameObject cyclops = NitroxEntity.RequireObjectFrom(id);
            CyclopsLightingPanel lighting = cyclops.RequireComponentInChildren<CyclopsLightingPanel>();
            if (lighting.lightingOn != isOn)
            {
                using (packetSender.Suppress<CyclopsToggleInternalLighting>())
                {
                    lighting.lightingOn = !lighting.lightingOn;
                    lighting.cyclopsRoot.ForceLightingState(lighting.lightingOn);
                    FMODAsset asset = (!lighting.lightingOn) ? lighting.vn_lightsOff : lighting.vn_lightsOn;
                    FMODUWE.PlayOneShot(asset, lighting.transform.position, 1f);
                    lighting.ReflectionCall("UpdateLightingButtons");
                }
            }
        }

        public void SetFloodLighting(NitroxId id, bool isOn)
        {
            GameObject cyclops = NitroxEntity.RequireObjectFrom(id);
            CyclopsLightingPanel lighting = cyclops.RequireComponentInChildren<CyclopsLightingPanel>();

            if (lighting.floodlightsOn != isOn)
            {
                using (packetSender.Suppress<CyclopsToggleFloodLights>())
                {
                    lighting.floodlightsOn = !lighting.floodlightsOn;
                    lighting.ReflectionCall("SetExternalLighting", false, false, new object[] { lighting.floodlightsOn });
                    FMODAsset asset = (!lighting.floodlightsOn) ? lighting.vn_floodlightsOff : lighting.vn_floodlightsOn;
                    FMODUWE.PlayOneShot(asset, lighting.transform.position, 1f);
                    lighting.ReflectionCall("UpdateLightingButtons");
                }
            }
        }

        public void ToggleEngineState(NitroxId id, bool isStarting, bool isOn, bool silent = false)
        {
            GameObject cyclops = NitroxEntity.RequireObjectFrom(id);
            CyclopsEngineChangeState engineState = cyclops.RequireComponentInChildren<CyclopsEngineChangeState>();
            CyclopsMotorMode motorMode = cyclops.RequireComponentInChildren<CyclopsMotorMode>();

            if (isOn == engineState.motorMode.engineOn)
            {
                if ((isStarting != (bool)engineState.ReflectionGet("startEngine")) != isOn)
                {
                    if (Player.main.currentSub != engineState.subRoot || silent)
                    {
                        engineState.ReflectionSet("startEngine", !isOn);
                        engineState.ReflectionSet("invalidButton", true);
                        engineState.Invoke("ResetInvalidButton", 2.5f);
                        engineState.subRoot.BroadcastMessage("InvokeChangeEngineState", !isOn, SendMessageOptions.RequireReceiver);
                    }
                    else
                    {
                        engineState.ReflectionSet("invalidButton", false);
                        using (packetSender.Suppress<CyclopsToggleInternalLighting>())
                        {
                            engineState.OnClick();
                        }
                    }
                }
            }
        }

        public void ChangeEngineMode(NitroxId id, CyclopsMotorMode.CyclopsMotorModes mode)
        {
            GameObject cyclops = NitroxEntity.RequireObjectFrom(id);
            CyclopsMotorMode motorMode = cyclops.RequireComponentInChildren<CyclopsMotorMode>();
            if (motorMode.cyclopsMotorMode != mode)
            {
                motorMode.BroadcastMessage("SetCyclopsMotorMode", mode, SendMessageOptions.RequireReceiver);
            }
        }

        public void ChangeSilentRunning(NitroxId id, bool isOn)
        {
            GameObject cyclops = NitroxEntity.RequireObjectFrom(id);
            CyclopsSilentRunningAbilityButton ability = cyclops.RequireComponentInChildren<CyclopsSilentRunningAbilityButton>();

            using (packetSender.Suppress<CyclopsChangeSilentRunning>())
            {
                if ((bool)ability.ReflectionGet("active") != isOn)
                {
                    Log.Debug("Set silent running to " + isOn + " for " + id);
                    ability.ReflectionSet("active", isOn);
                    if (isOn)
                    {
                        ability.image.sprite = ability.activeSprite;
                        ability.subRoot.BroadcastMessage("RigForSilentRunning");
                        ability.InvokeRepeating("SilentRunningIteration", 0f, ability.silentRunningIteration);
                    }
                    else
                    {
                        ability.image.sprite = ability.inactiveSprite;
                        ability.subRoot.BroadcastMessage("SecureFromSilentRunning");
                        ability.CancelInvoke("SilentRunningIteration");
                    }
                }
            }
        }

        public void ChangeShieldMode(NitroxId id, bool isOn)
        {
            GameObject cyclops = NitroxEntity.RequireObjectFrom(id);
            CyclopsShieldButton shield = cyclops.GetComponentInChildren<CyclopsShieldButton>();
            if (shield != null)
            {
                using (packetSender.Suppress<CyclopsChangeShieldMode>())
                {
                    if ((shield.activeSprite == shield.image.sprite) != isOn)
                    {
                        if (isOn)
                        {
                            shield.ReflectionCall("StartShield");
                        }
                        else
                        {
                            shield.ReflectionCall("StopShield");
                        }
                    }
                }
            }
        }

        public void ChangeSonarMode(NitroxId id, bool isOn)
        {
            GameObject cyclops = NitroxEntity.RequireObjectFrom(id);
            CyclopsSonarButton sonar = cyclops.GetComponentInChildren<CyclopsSonarButton>();
            if (sonar != null)
            {
                using (packetSender.Suppress<CyclopsChangeSonarMode>())
                {
                    // At this moment the code is "non functional" as for some reason changing the sprite will never happen
                    // Also setting sonar as active will never work

                    MethodInfo sonarSetActiveInfo = sonar.GetType().GetMethod("set_sonarActive", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (sonarSetActiveInfo != null)
                    {
                        sonarSetActiveInfo.Invoke(sonar, new object[] { isOn });
                    }
                    if (isOn)
                    {
                        sonar.image.sprite = sonar.activeSprite;
                    }
                    else
                    {
                        sonar.image.sprite = sonar.inactiveSprite;
                    }
                }
            }
        }

        public void SonarPing(NitroxId id)
        {
            GameObject cyclops = NitroxEntity.RequireObjectFrom(id);
            CyclopsSonarButton sonar = cyclops.GetComponentInChildren<CyclopsSonarButton>();
            if (sonar != null)
            {
                using (packetSender.Suppress<CyclopsSonarPing>())
                {
                    sonar.ReflectionCall("SonarPing");
                }
            }
        }

        public void LaunchDecoy(NitroxId id)
        {
            GameObject cyclops = NitroxEntity.RequireObjectFrom(id);
            CyclopsDecoyManager decoyManager = cyclops.RequireComponent<CyclopsDecoyManager>();
            using (packetSender.Suppress<CyclopsChangeSilentRunning>())
            {
                decoyManager.Invoke("LaunchWithDelay", 3f);
                decoyManager.decoyLaunchButton.UpdateText();
                decoyManager.subRoot.voiceNotificationManager.PlayVoiceNotification(decoyManager.subRoot.decoyNotification, false, true);
                decoyManager.subRoot.BroadcastMessage("UpdateTotalDecoys", decoyManager.decoyCount, SendMessageOptions.DontRequireReceiver);
                CyclopsDecoyLaunchButton decoyLaunchButton = cyclops.RequireComponent<CyclopsDecoyLaunchButton>();
                decoyLaunchButton.StartCooldown();
            }
        }

        public void StartFireSuppression(NitroxId id)
        {
            GameObject cyclops = NitroxEntity.RequireObjectFrom(id);
            CyclopsFireSuppressionSystemButton fireSuppButton = cyclops.RequireComponentInChildren<CyclopsFireSuppressionSystemButton>();
            using (packetSender.Suppress<CyclopsFireSuppression>())
            {
                // Infos from SubFire.StartSystem
                fireSuppButton.subFire.StartCoroutine(StartFireSuppressionSystem(fireSuppButton.subFire));
                fireSuppButton.StartCooldown();
            }
        }

        // Remake of the StartSystem Coroutine from original player. Some Methods are not used from the original coroutine
        // For example no temporaryClose as this will be initiated anyway from the originating Player
        // Also the fire extiguishing will not start cause the initial player is already extiguishing the fires. Else this could double/triple/... the extinguishing
        private IEnumerator StartFireSuppressionSystem(SubFire fire)
        {
            fire.subRoot.voiceNotificationManager.PlayVoiceNotification(fire.subRoot.fireSupressionNotification, false, true);
            yield return new WaitForSeconds(3f);
            fire.ReflectionSet("fireSuppressionActive", true);
            fire.subRoot.fireSuppressionState = true;
            fire.subRoot.BroadcastMessage("NewAlarmState", null, SendMessageOptions.DontRequireReceiver);
            fire.Invoke("CancelFireSuppression", fire.fireSuppressionSystemDuration);
            float doorCloseDuration = 30f;
            fire.gameObject.BroadcastMessage("TemporaryLock", doorCloseDuration, SendMessageOptions.DontRequireReceiver);
            yield break;
        }


        internal void SetStandardModes(CyclopsModel cyclopsModel)
        {
            SetInternalLighting(cyclopsModel.Id, cyclopsModel.InternalLightsOn);
            SetFloodLighting(cyclopsModel.Id, cyclopsModel.FloodLightsOn);
            ToggleEngineState(cyclopsModel.Id, cyclopsModel.EngineState, false, true);
            ChangeEngineMode(cyclopsModel.Id, cyclopsModel.EngineMode);
        }

        // Will be called at a later, because it is needed that the modules are installed and may need power to not immediatly be shut off
        public void SetAdvancedModes(CyclopsModel cyclopsModel)
        {
            // We need to wait till the cyclops is powered up to start all advanced modes
            // At this time all Equipment will be loaded into the cyclops, so we do not need other structures
            SubRoot root = NitroxEntity.GetObjectFrom(cyclopsModel.Id).Value.GetComponent<SubRoot>();
            UWE.Event<PowerRelay>.HandleFunction handleFunction = null;
            handleFunction = new UWE.Event<PowerRelay>.HandleFunction((relay) =>
            {
                ChangeSilentRunning(cyclopsModel.Id, cyclopsModel.SilentRunningOn);
                ChangeShieldMode(cyclopsModel.Id, cyclopsModel.ShieldOn);
                ChangeSonarMode(cyclopsModel.Id, cyclopsModel.SonarOn);

                // After registering all modes. Remove the handler
                root.powerRelay.powerUpEvent.RemoveHandler(root, handleFunction);
            });
            root.powerRelay.powerUpEvent.AddHandler(root, handleFunction);

        }

        public void SetAllModes(CyclopsModel model)
        {
            SetStandardModes(model);
            SetAdvancedModes(model);
        }

        /// <summary>
        /// Triggers a <see cref="CyclopsDamage"/> packet
        /// </summary>
        public void OnCreateDamagePoint(SubRoot subRoot)
        {
            BroadcastDamageState(subRoot, Optional.Empty);
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
            NitroxId subId = NitroxEntity.GetId(subRoot.gameObject);

            for (int i = 0; i < subRoot.damageManager.damagePoints.Length; i++)
            {
                if (subRoot.damageManager.damagePoints[i] == damagePoint)
                {
                    CyclopsDamagePointRepaired packet = new CyclopsDamagePointRepaired(subId, i, repairAmount);
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
            NitroxId subId = NitroxEntity.GetId(subRoot.gameObject);
            LiveMixin subHealth = subRoot.gameObject.RequireComponent<LiveMixin>();

            if (subHealth.health > 0)
            {
                CyclopsDamageInfoData damageInfo = null;

                if (info.HasValue)
                {
                    DamageInfo damage = info.Value;
                    // Source of the damage. Used if the damage done to the Cyclops was not calculated on other clients. Currently it's just used to figure out what sounds and
                    // visual effects should be used.
                    CyclopsDamageInfoData serializedDamageInfo = new CyclopsDamageInfoData(subId,
                        damage.dealer != null ? NitroxEntity.GetId(damage.dealer) : null,
                        damage.originalDamage,
                        damage.damage,
                        damage.position,
                        damage.type);
                }

                int[] damagePointIndexes = GetActiveDamagePoints(subRoot).ToArray();
                CyclopsFireData[] firePoints = GetActiveRoomFires(subRoot.GetComponent<SubFire>()).ToArray();

                CyclopsDamage packet = new CyclopsDamage(subId, subRoot.GetComponent<LiveMixin>().health, subRoot.damageManager.subLiveMixin.health, subRoot.GetComponent<SubFire>().liveMixin.health, damagePointIndexes, firePoints, damageInfo);
                packetSender.Send(packet);
            }
            else
            {
                // RIP
                CyclopsDestroyed packet = new CyclopsDestroyed(subId);
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
            NitroxId subRootId = NitroxEntity.GetId(subFire.subRoot.gameObject);
            Dictionary<CyclopsRooms, SubFire.RoomFire> roomFires = (Dictionary<CyclopsRooms, SubFire.RoomFire>)subFire.ReflectionGet("roomFires");

            foreach (KeyValuePair<CyclopsRooms, SubFire.RoomFire> roomFire in roomFires)
            {
                for (int i = 0; i < roomFire.Value.spawnNodes.Length; i++)
                {
                    if (roomFire.Value.spawnNodes[i].childCount > 0)
                    {
                        yield return new CyclopsFireData(NitroxEntity.GetId(roomFire.Value.spawnNodes[i].GetComponentInChildren<Fire>().gameObject),
                            subRootId,
                            roomFire.Key,
                            i);
                    }
                }
            }
        }
    }
}
