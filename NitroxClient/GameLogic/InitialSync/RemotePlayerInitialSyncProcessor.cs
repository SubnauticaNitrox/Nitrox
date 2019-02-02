using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync
{
    public class RemotePlayerInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly PlayerManager remotePlayerManager;

        public RemotePlayerInitialSyncProcessor(PlayerManager remotePlayerManager)
        {
            this.remotePlayerManager = remotePlayerManager;

            DependentProcessors.Add(typeof(BuildingInitialSyncProcessor)); // Remote players can be spawned inside buildings.
            DependentProcessors.Add(typeof(EscapePodInitialSyncProcessor)); // Remote players can be spawned inside the escape pod.
            DependentProcessors.Add(typeof(VehicleInitialSyncProcessor)); // Remote players can be piloting vehicles.
        }

        public override void Process(InitialPlayerSync packet)
        {
            foreach (InitialRemotePlayerData playerData in packet.RemotePlayerData)
            {
                RemotePlayer player = remotePlayerManager.Create(playerData.PlayerContext, playerData.EquippedTechTypes);

                if (playerData.SubRootGuid.IsPresent())
                {
                    Optional<GameObject> sub = GuidHelper.GetObjectFrom(playerData.SubRootGuid.Get());

                    if (sub.IsPresent())
                    {
                        player.SetSubRoot(sub.Get().GetComponent<SubRoot>());
                    }
                    else
                    {
                        Log.Error("Could not spawn remote player into subroot with guid: " + playerData.SubRootGuid.Get());
                    }
                }
            }
        }
    }
}
