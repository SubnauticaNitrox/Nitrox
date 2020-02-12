using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
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
                List<TechType> equippedTechTypes = playerData.EquippedTechTypes.Select(techType => techType.Enum()).ToList();
                RemotePlayer player = remotePlayerManager.Create(playerData.PlayerContext, equippedTechTypes);

                if (playerData.SubRootId.IsPresent())
                {
                    Optional<GameObject> sub = NitroxEntity.GetObjectFrom(playerData.SubRootId.Get());

                    if (sub.IsPresent())
                    {
                        player.SetSubRoot(sub.Get().GetComponent<SubRoot>());
                    }
                    else
                    {
                        Log.Instance.LogMessage(LogCategory.Error, "Could not spawn remote player into subroot with id: " + playerData.SubRootId.Get());
                    }
                }
            }
        }
    }
}
