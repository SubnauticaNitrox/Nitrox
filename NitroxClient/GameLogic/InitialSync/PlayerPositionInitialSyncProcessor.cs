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

        public override void Process(InitialPlayerSync packet)
        {
            Vector3 position = packet.PlayerSpawnData;
            Optional<NitroxId> subRootId = packet.PlayerSubRootId;

            if (position.x == 0 && position.y == 0 && position.z == 0)
            {
                position = Player.mainObject.transform.position;
            }
            Player.main.SetPosition(position);

            if (subRootId.IsPresent())
            {
                Optional<GameObject> sub = NitroxEntity.GetObjectFrom(subRootId.Get());

                if (sub.IsPresent())
                {
                    SubRoot root = sub.Get().GetComponent<SubRoot>();
                    // Player position is relative to a subroot if in a subroot
                    if (root != null && !root.isBase)
                    {                        
                        Player.main.SetCurrentSub(root);
                        Quaternion vehicleAngle = root.transform.rotation;
                        position = vehicleAngle * position;
                        position = position + root.transform.position;
                        Player.main.SetPosition(position);
                    }
                    else
                    {
                        Log.Instance.LogMessage(LogCategory.Error, "Could not find subroot for player for subroot with id: " + subRootId.Get());
                    }
                }
                else
                {
                    Log.Instance.LogMessage(LogCategory.Error, "Could not spawn player into subroot with id: " + subRootId.Get());
                }
            }
        }
    }
}
