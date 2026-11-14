using System;
using System.Collections;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using UnityEngine;
using static NitroxClient.GameLogic.Spawning.Metadata.Extractor.CyclopsMetadataExtractor;

namespace NitroxClient.GameLogic
{
    public class Cyclops
    {
        private readonly Entities entities;
        private readonly EntityMetadataManager entityMetadataManager;
        private readonly IPacketSender packetSender;

        public Cyclops(IPacketSender packetSender, EntityMetadataManager entityMetadataManager, Entities entities)
        {
            this.packetSender = packetSender;
            this.entities = entities;
            this.entityMetadataManager = entityMetadataManager;
        }

        public void BroadcastMetadataChange(NitroxId id)
        {
            SubRoot subRoot = NitroxEntity.RequireObjectFrom(id).RequireComponent<SubRoot>();
            entities.EntityMetadataChanged(subRoot, id);
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
                decoyManager.subRoot.BroadcastMessage(nameof(CyclopsDecoyManager.UpdateTotalDecoys), decoyManager.decoyCount, SendMessageOptions.DontRequireReceiver);
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

        public void OnCreateDamagePoint(SubRoot subRoot, CyclopsDamagePoint damagePoint, int damagePointIndex)
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

            NitroxId id = NitroxEntity.GenerateNewId(damagePoint.gameObject);
            Optional<EntityMetadata> metadata = entityMetadataManager.Extract(damagePoint);
            CyclopsDamagePointEntity entity = new(damagePointIndex, id, metadata.OrNull(), subId);

            EntitySpawnedByClient packet = new(entity);
            packetSender.Send(packet);
        }

        public void OnDamagePointRepaired(CyclopsDamagePoint damagePoint)
        {
            if (!damagePoint.TryGetIdOrWarn(out NitroxId id))
            {
                return;
            }

            NitroxEntity.RemoveFrom(damagePoint.gameObject);

            EntityDestroyed packet = new(id);
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
            fire.subRoot.BroadcastMessage(nameof(SubFloodAlarm.NewAlarmState), null, SendMessageOptions.DontRequireReceiver);
            fire.Invoke(nameof(SubFire.CancelFireSuppression), fire.fireSuppressionSystemDuration);
            float doorCloseDuration = 30f;
            fire.gameObject.BroadcastMessage(nameof(Openable.TemporaryLock), doorCloseDuration, SendMessageOptions.DontRequireReceiver);
        }
    }
}
