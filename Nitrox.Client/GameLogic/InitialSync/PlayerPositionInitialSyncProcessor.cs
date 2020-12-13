using System;
using System.Collections;
using Nitrox.Client.GameLogic.InitialSync.Base;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Logger;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;
using UnityEngine;

namespace Nitrox.Client.GameLogic.InitialSync
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
            Vector3 position = packet.PlayerSpawnData.ToUnity();
            if (Math.Abs(position.x) < 0.0002 && Math.Abs(position.y) < 0.0002 && Math.Abs(position.z) < 0.0002)
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
