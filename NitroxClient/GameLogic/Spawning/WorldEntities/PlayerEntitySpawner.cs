using System.Collections;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class PlayerEntitySpawner : SyncEntitySpawner<PlayerEntity>
{
    private readonly PlayerManager playerManager;
    private readonly ILocalNitroxPlayer localPlayer;

    public PlayerEntitySpawner(PlayerManager playerManager, ILocalNitroxPlayer localPlayer)
    {
        this.playerManager = playerManager;
        this.localPlayer = localPlayer;
    }

    protected override IEnumerator SpawnAsync(PlayerEntity entity, TaskResult<Optional<GameObject>> result)
    {
        SpawnSync(entity, result);
        return null;
    }

    protected override bool SpawnSync(PlayerEntity entity, TaskResult<Optional<GameObject>> result)
    {
        if (Player.main.TryGetNitroxId(out NitroxId localPlayerId) && localPlayerId == entity.Id)
        {
            // No special setup for the local player.  Simply return saying it is spawned.
            result.Set(Player.main.gameObject);
            return true;
        }

        Optional<RemotePlayer> remotePlayer = playerManager.Find(entity.Id);
        Optional<GameObject> parent = entity.ParentId != null ? NitroxEntity.GetObjectFrom(entity.Id) : Optional.Empty;

        // The server may send us a player entity but they are not guarenteed to be actively connected at the moment - don't spawn them.  In the
        // future, we could make this configurable to be able to spawn disconnected players in the world.
        if (!remotePlayer.HasValue || remotePlayer.Value.Body)
        {
            result.Set(Optional.Empty);
            return true;
        }

        GameObject remotePlayerBody = CloneLocalPlayerBodyPrototype();
        remotePlayer.Value.InitializeGameObject(remotePlayerBody);

        if (parent.HasValue)
        {
            AttachToParent(remotePlayer.Value, parent.Value);
        }

        result.Set(Optional.Of(remotePlayerBody));
        return true;
    }

    protected override bool SpawnsOwnChildren(PlayerEntity entity) => false;

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
}
