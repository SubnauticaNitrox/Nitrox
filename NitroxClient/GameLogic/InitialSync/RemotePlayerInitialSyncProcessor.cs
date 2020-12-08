using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
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
                        if (subroot)
                        {
                            Log.Debug("Found subroot for player. Will add him and update animation.");
                            player.SetSubRoot(subroot);                            
                        }
                    }
                    else
                    {
                        Log.Error("Could not spawn remote player into subroot/escape pod with id: " + playerData.SubRootId.Value);
                    }
                }
                
                if (!IsSwimming(playerData.Position.ToUnity(), playerData.SubRootId))
                {
                    player.UpdateAnimation(AnimChangeType.UNDERWATER, AnimChangeState.OFF);
                }
                remotePlayersSynced++;
                yield return null;
            }
        }

        private bool IsSwimming(Vector3 playerPosition, Optional<NitroxId> subId)
        {
            if (subId.HasValue)
            {
                Optional<GameObject> sub = NitroxEntity.GetObjectFrom(subId.Value);
                SubRoot subroot = null;
                sub.Value.TryGetComponent<SubRoot>(out subroot);
                // Set the animation for the remote player to standing instead of swimming if player is not in a flooded subroot
                // or in a waterpark                            
                if (subroot)
                {
                    if (subroot.IsUnderwater(playerPosition))
                    {
                        return true;
                    }
                    if (subroot.isCyclops)
                    {
                        return false;
                    }
                    // We know that we are in a subroot. But we can also be in a waterpark in a subroot, where we would swim
                    BaseRoot baseRoot = subroot.GetComponentInParent<BaseRoot>();
                    if (baseRoot)
                    {
                        WaterPark[] waterParks = baseRoot.GetComponentsInChildren<WaterPark>();
                        foreach (WaterPark waterPark in waterParks)
                        {
                            if (waterPark.IsPointInside(playerPosition))
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                }

                Log.Debug($"Trying to find escape pod for {subId}.");
                EscapePod escapePod = null;
                sub.Value.TryGetComponent<EscapePod>(out escapePod);
                if (escapePod)
                {
                    Log.Debug("Found escape pod for player. Will add him and update animation.");
                    return false;
                }
            }
            // Player can be above ocean level.
            float oceanLevel = Ocean.main.GetOceanLevel();
            return playerPosition.y < oceanLevel;
        }
    }
}
