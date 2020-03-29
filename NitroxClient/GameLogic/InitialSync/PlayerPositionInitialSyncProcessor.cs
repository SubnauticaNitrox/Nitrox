using System.Collections;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync
{
    public class PlayerPositionInitialSyncProcessor : InitialSyncProcessor
    {
        public PlayerPositionInitialSyncProcessor()
        {
            DependentProcessors.Add(typeof(PlayerInitialSyncProcessor)); // Make sure the player is configured
            DependentProcessors.Add(typeof(BuildingInitialSyncProcessor)); // Players can be spawned in buildings
            DependentProcessors.Add(typeof(EscapePodInitialSyncProcessor)); // Players can be spawned in escapePod
            DependentProcessors.Add(typeof(VehicleInitialSyncProcessor)); // Players can be spawned in vehicles
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            Vector3 position = packet.PlayerSpawnData;
            if (position.x == 0 && position.y == 0 && position.z == 0)
            {
                position = Player.mainObject.transform.position;
            }
            Player.main.SetPosition(position);
            
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
            SubRoot root = sub.Value.GetComponent<SubRoot>();
            if (root == null)
            {
                Log.Error("Could not find subroot for player for subroot with id: " + subRootId.Value);
                yield break;
            }
            
            // If player is not swimming
            Player.main.SetCurrentSub(root);
            if (root.isBase)
            {
                yield break;
            }
            Transform rootTransform = root.transform;
            Quaternion vehicleAngle = rootTransform.rotation;
            position = vehicleAngle * position;
            position = position + rootTransform.position;
            Player.main.SetPosition(position);
        }
    }
}
