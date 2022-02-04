using System;
using System.Collections;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync
{
    public class PlayerPositionInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly IPacketSender packetSender;

        public PlayerPositionInitialSyncProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;

            DependentProcessors.Add(typeof(PlayerInitialSyncProcessor)); // Make sure the player is configured
            DependentProcessors.Add(typeof(BuildingInitialSyncProcessor)); // Players can be spawned in buildings
            DependentProcessors.Add(typeof(EscapePodInitialSyncProcessor)); // Players can be spawned in escapePod
            DependentProcessors.Add(typeof(VehicleInitialSyncProcessor)); // Players can be spawned in vehicles
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            Vector3 position = packet.PlayerSpawnData.ToUnity();
            if (Math.Abs(position.x) < 0.0002 && Math.Abs(position.y) < 0.0002 && Math.Abs(position.z) < 0.0002)
            {
                position = Player.mainObject.transform.position;
            }
            Player.main.SetPosition(position);

            // Player.Update is setting SubRootID to null after Player position is set
            using (packetSender.Suppress<EscapePodChanged>())
            {
                Player.main.ValidateEscapePod();
            }

            // Player position is relative to a subroot if in a subroot
            Optional<NitroxId> subRootId = packet.PlayerSubRootId;
            if (!subRootId.HasValue)
            {
                yield break;
            }

            Optional<GameObject> sub = NitroxEntity.GetObjectFrom(subRootId.Value);
            if (!sub.HasValue)
            {
                Log.Error("Could not spawn player into subroot with id: " + subRootId.Value);
                yield break;
            }

            if (!sub.Value.TryGetComponent(out SubRoot subRoot))
            {
                Log.Debug("SubRootId-GameObject has no SubRoot component, so it's assumed to be the EscapePod");
                yield break;
            }

            // If player is not swimming
            Player.main.SetCurrentSub(subRoot);
            if (subRoot.isBase)
            {
                yield break;
            }
            Transform rootTransform = subRoot.transform;
            Quaternion vehicleAngle = rootTransform.rotation;
            position = vehicleAngle * position;
            position = position + rootTransform.position;
            Player.main.SetPosition(position);
        }
    }
}
