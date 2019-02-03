using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.InitialSync.Base;
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
            Optional<string> subRootGuid = packet.PlayerSubRootGuid;

            if (!(position.x == 0 && position.y == 0 && position.z == 0))
            {
                Player.main.SetPosition(position);
            }

            if (subRootGuid.IsPresent())
            {
                Optional<GameObject> sub = GuidHelper.GetObjectFrom(subRootGuid.Get());

                if (sub.IsPresent())
                {
                    Player.main.SetCurrentSub(sub.Get().GetComponent<SubRoot>());
                }
                else
                {
                    Log.Error("Could not spawn player into subroot with guid: " + subRootGuid.Get());
                }
            }
        }
    }
}
