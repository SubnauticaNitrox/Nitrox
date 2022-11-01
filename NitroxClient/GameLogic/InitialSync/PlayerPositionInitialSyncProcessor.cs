using System.Collections;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using Math = System.Math;

namespace NitroxClient.GameLogic.InitialSync
{
    public class PlayerPositionInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly IPacketSender packetSender;
        private readonly Terrain terrain;

        public PlayerPositionInitialSyncProcessor(IPacketSender packetSender, Terrain terrain)
        {
            this.packetSender = packetSender;
            this.terrain = terrain;

            DependentProcessors.Add(typeof(PlayerInitialSyncProcessor)); // Make sure the player is configured
            DependentProcessors.Add(typeof(BuildingInitialSyncProcessor)); // Players can be spawned in buildings
            DependentProcessors.Add(typeof(EscapePodInitialSyncProcessor)); // Players can be spawned in escapePod
            DependentProcessors.Add(typeof(VehicleInitialSyncProcessor)); // Players can be spawned in vehicles
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            // We freeze the player so that he doesn't fall before the cells around him have loaded
            Player.main.cinematicModeActive = true;

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
                yield return terrain.WaitForWorldLoad();
                yield break;
            }

            Optional<GameObject> sub = NitroxEntity.GetObjectFrom(subRootId.Value);
            if (!sub.HasValue)
            {
                Log.Error("Could not spawn player into subroot with id: " + subRootId.Value);
                yield return terrain.WaitForWorldLoad();
                yield break;
            }

            if (!sub.Value.TryGetComponent(out SubRoot subRoot))
            {
                Log.Debug("SubRootId-GameObject has no SubRoot component, so it's assumed to be the EscapePod");
                yield return terrain.WaitForWorldLoad();
                yield break;
            }

            // If player is not swimming
            Player.main.SetCurrentSub(subRoot);
            if (subRoot.isBase)
            {
                // If the player's in a base, we don't need to wait for the world to load
                yield break;
            }
            Transform rootTransform = subRoot.transform;
            Quaternion vehicleAngle = rootTransform.rotation;
            position = vehicleAngle * position;
            position = position + rootTransform.position;
            Player.main.SetPosition(position);
            yield return terrain.WaitForWorldLoad();
        }
    }
}
