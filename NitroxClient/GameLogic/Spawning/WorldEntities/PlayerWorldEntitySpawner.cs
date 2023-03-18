using System.Collections;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class PlayerWorldEntitySpawner : IWorldEntitySpawner
{
    private readonly PlayerManager playerManager;
    private readonly ILocalNitroxPlayer localPlayer;

    public PlayerWorldEntitySpawner(PlayerManager playerManager, ILocalNitroxPlayer localPlayer)
    {
        this.playerManager = playerManager;
        this.localPlayer = localPlayer;
    }

    public IEnumerator SpawnAsync(WorldEntity entity, Optional<GameObject> parent, EntityCell cellRoot, TaskResult<Optional<GameObject>> result)
    {
        NitroxId localPlayer = NitroxEntity.GetId(Player.main.gameObject);

        if (localPlayer == entity.Id)
        {
            // No special setup for the local player.  Simply return saying it is spawned.
            result.Set(Player.main.gameObject);
            yield break;
        }

        Optional<RemotePlayer> remotePlayer = playerManager.Find(entity.Id);

        // The server may send us a player entity but they are not guarenteed to be actively connected at the moment - don't spawn them.  In the
        // future, we could make this configurable to be able to spawn disconnected players in the world.
        if (remotePlayer.HasValue)
        {
            GameObject remotePlayerBody = CloneLocalPlayerBodyPrototype();

            remotePlayer.Value.InitializeGameObject(remotePlayerBody);
            
            if (!IsSwimming(entity.Transform.Position.ToUnity(), parent))
            {
                remotePlayer.Value.UpdateAnimationAndCollider(AnimChangeType.UNDERWATER, AnimChangeState.OFF);
            }

            if (parent.HasValue)
            {
                AttachToParent(remotePlayer.Value, parent.Value);
            }

            result.Set(Optional.Of(remotePlayerBody));
            yield break;
        }

        result.Set(Optional.Empty);
    }

    public bool SpawnsOwnChildren()
    {
        return false;
    }

    private GameObject CloneLocalPlayerBodyPrototype()
    {
        GameObject clone = Object.Instantiate(localPlayer.BodyPrototype, null, false);
        clone.SetActive(true);
        return clone;
    }

    private void AttachToParent(RemotePlayer remotePlayer, GameObject parent)
    {
        if (parent.TryGetComponent(out SubRoot subRoot))
        {
            Log.Debug($"Found sub root for {remotePlayer.PlayerName}. Will add him and update animation.");
            remotePlayer.SetSubRoot(subRoot);
        }
        else if (parent.TryGetComponent(out EscapePod escapePod))
        {
            Log.Debug($"Found EscapePod for {remotePlayer.PlayerName}.");
            remotePlayer.SetEscapePod(escapePod);
        }
        else
        {
            Log.Error($"Found neither SubRoot component nor EscapePod on {parent.name} for {remotePlayer.PlayerName}.");
        }        
    }

    private bool IsSwimming(Vector3 playerPosition, Optional<GameObject> parent)
    {
        if (parent.HasValue)
        {
            parent.Value.TryGetComponent<SubRoot>(out SubRoot subroot);

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

            Log.Debug($"Trying to find escape pod for {parent}.");
            parent.Value.TryGetComponent<EscapePod>(out EscapePod escapePod);
            if (escapePod)
            {
                Log.Debug("Found escape pod for player. Will add him and update animation.");
                return false;
            }
        }

        // Player can be above ocean level.
        float oceanLevel = Ocean.GetOceanLevel();
        return playerPosition.y < oceanLevel;
    }
}
