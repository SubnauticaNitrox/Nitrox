using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
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

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            int remotePlayersSynced = 0;

            foreach (InitialRemotePlayerData playerData in packet.RemotePlayerData)
            {
                waitScreenItem.SetProgress(remotePlayersSynced, packet.RemotePlayerData.Count);

                List<TechType> equippedTechTypes = playerData.EquippedTechTypes.Select(techType => techType.ToUnity()).ToList();
                RemotePlayer player = remotePlayerManager.Create(playerData.PlayerContext, equippedTechTypes);

                if (playerData.SubRootId.HasValue)
                {
                    Optional<GameObject> sub = NitroxEntity.GetObjectFrom(playerData.SubRootId.Value);

                    if (sub.HasValue)
                    {
                        Log.Debug($"sub value set to {sub.Value}. Try to find subroot");
                        SubRoot subroot = null;
                        sub.Value.TryGetComponent<SubRoot>(out subroot);
                        if (subroot != null)
                        {
                            Log.Debug("Found subroot for player. Will add him and update animation.");
                            player.SetSubRoot(subroot);
                            // Set the animation for the remote player to standing instead of swimming if player is not in a flooded subroot
                            if (!subroot.IsUnderwater(player.Body.transform.position))
                            {
                                player.UpdateAnimation(AnimChangeType.UNDERWATER, AnimChangeState.OFF);
                            }
                        }
                        Log.Debug("Trying to find escape pod.");
                        EscapePod escapePod = null;
                        sub.Value.TryGetComponent<EscapePod>(out escapePod);
                        if (escapePod != null)
                        {
                            Log.Debug("Found escape pod for player. Will add him and update animation.");
                            player.UpdateAnimation(AnimChangeType.UNDERWATER, AnimChangeState.OFF);
                        }
                    }
                    else
                    {
                        Log.Error("Could not spawn remote player into subroot/escape pod with id: " + playerData.SubRootId.Value);
                    }
                }

                remotePlayersSynced++;
                yield return null;
            }
        }
    }
}
