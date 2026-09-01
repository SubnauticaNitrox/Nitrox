using System;
using System.Collections;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using UnityEngine;
using static NitroxClient.GameLogic.Spawning.Metadata.Extractor.CyclopsMetadataExtractor;

namespace NitroxClient.GameLogic
{
    public class Cyclops
    {
        private readonly Entities entities;
        private readonly IPacketSender packetSender;

        public Cyclops(IPacketSender packetSender, Entities entities)
        {
            this.packetSender = packetSender;
            this.entities = entities;
        }

        public void BroadcastMetadataChange(NitroxId id)
        {
            GameObject gameObject = NitroxEntity.RequireObjectFrom(id);
            CyclopsGameObject cyclops = new() { GameObject = gameObject };
            entities.EntityMetadataChanged(cyclops, id);
        }

        public void BroadcastLaunchDecoy(NitroxId id)
        {
            CyclopsDecoyLaunch packet = new(id);
            packetSender.Send(packet);
        }

        public void BroadcastActivateFireSuppression(NitroxId id)
        {
            CyclopsFireSuppression packet = new(id);
            packetSender.Send(packet);
        }

        public void LaunchDecoy(NitroxId id)
        {
            GameObject cyclops = NitroxEntity.RequireObjectFrom(id);
            CyclopsDecoyManager decoyManager = cyclops.RequireComponent<CyclopsDecoyManager>();
            using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
            {
                decoyManager.Invoke(nameof(CyclopsDecoyManager.LaunchWithDelay), 3f);
                decoyManager.decoyLaunchButton.UpdateText();
                decoyManager.subRoot.voiceNotificationManager.PlayVoiceNotification(decoyManager.subRoot.decoyNotification, false, true);
                decoyManager.subRoot.BroadcastMessage("UpdateTotalDecoys", decoyManager.decoyCount, SendMessageOptions.DontRequireReceiver);
                CyclopsDecoyLaunchButton decoyLaunchButton = cyclops.RequireComponentInChildren<CyclopsDecoyLaunchButton>();
                decoyLaunchButton.StartCooldown();
            }
        }

        public void StartFireSuppression(NitroxId id)
        {
            GameObject cyclops = NitroxEntity.RequireObjectFrom(id);
            CyclopsFireSuppressionSystemButton fireSuppButton = cyclops.RequireComponentInChildren<CyclopsFireSuppressionSystemButton>();
            using (PacketSuppressor<CyclopsFireSuppression>.Suppress())
            {
                // Infos from SubFire.StartSystem
                fireSuppButton.subFire.StartCoroutine(StartFireSuppressionSystem(fireSuppButton.subFire));
                fireSuppButton.StartCooldown();
            }
        }

        /// <summary>
        ///     Triggers a <see cref="CyclopsDamagePointCreated" /> packet
        /// </summary>
        public void OnCreateDamagePoint(SubRoot subRoot, int damagePointIndex)
        {
            if (!subRoot.TryGetIdOrWarn(out NitroxId subId))
            {
                return;
            }

            LiveMixin subHealth = subRoot.gameObject.RequireComponent<LiveMixin>();
            if (subHealth.health <= 0)
            {
                return;
            }

            CyclopsDamagePointCreated packet = new(subId, damagePointIndex);
            packetSender.Send(packet);
        }

        /// <summary>
        ///     Called when the player repairs a <see cref="CyclopsDamagePoint" />. Right now it's not possible to partially repair
        ///     because it would be difficult to implement.
        ///     <see cref="CyclopsDamagePoint" />s are coupled with <see cref="LiveMixin" />, which is used with just about
        ///     anything that has health.
        ///     I would need to hook onto <see cref="LiveMixin.AddHealth(float)" />, or maybe the repair gun event to catch when
        ///     something repairs a damage point, which I don't
        ///     believe is worth the effort. A <see cref="CyclopsDamagePoint" /> is already fully repaired in a little over a
        ///     second. This can trigger sending
        ///     <see cref="CyclopsDamagePointRepaired" /> and <see cref="CyclopsDamagePointCreated" /> packets
        /// </summary>
        public void OnDamagePointRepaired(SubRoot subRoot, CyclopsDamagePoint damagePoint, float repairAmount)
        {
            if (!subRoot.TryGetIdOrWarn(out NitroxId subId))
            {
                return;
            }

            int index = Array.IndexOf(subRoot.damageManager.damagePoints, damagePoint);

            CyclopsDamagePointRepaired packet = new(subId, index, repairAmount);
            packetSender.Send(packet);
        }

        // Remake of the StartSystem Coroutine from original player. Some Methods are not used from the original coroutine
        // For example no temporaryClose as this will be initiated anyway from the originating Player
        // Also the fire extiguishing will not start cause the initial player is already extiguishing the fires. Else this could double/triple/... the extinguishing
        private IEnumerator StartFireSuppressionSystem(SubFire fire)
        {
            fire.subRoot.voiceNotificationManager.PlayVoiceNotification(fire.subRoot.fireSupressionNotification, false, true);
            yield return Yielders.WaitFor3Seconds;
            fire.fireSuppressionActive = true;
            fire.subRoot.fireSuppressionState = true;
            fire.subRoot.BroadcastMessage("NewAlarmState", null, SendMessageOptions.DontRequireReceiver);
            fire.Invoke(nameof(SubFire.CancelFireSuppression), fire.fireSuppressionSystemDuration);
            float doorCloseDuration = 30f;
            fire.gameObject.BroadcastMessage("TemporaryLock", doorCloseDuration, SendMessageOptions.DontRequireReceiver);
        }
    }
}
