using System.Collections;
using NitroxClient.GameLogic;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerJoinedMultiplayerSessionProcessor : IClientPacketProcessor<PlayerJoinedMultiplayerSession>
{
    private readonly PlayerManager playerManager;
    private readonly Entities entities;

    public PlayerJoinedMultiplayerSessionProcessor(PlayerManager playerManager, Entities entities)
    {
        this.playerManager = playerManager;
        this.entities = entities;
    }

    public Task Process(IPacketProcessContext context, PlayerJoinedMultiplayerSession packet)
    {
        CoroutineHost.StartCoroutine(SpawnRemotePlayer(packet));
        return Task.CompletedTask;
    }

    private IEnumerator SpawnRemotePlayer(PlayerJoinedMultiplayerSession packet)
    {
        playerManager.Create(packet.PlayerContext);
        yield return entities.SpawnEntityAsync(packet.PlayerWorldEntity, true, true);

        Log.Info($"{packet.PlayerContext.PlayerName} joined the game");
        Log.InGame(Language.main.Get("Nitrox_PlayerJoined").Replace("{PLAYER}", packet.PlayerContext.PlayerName));
    }
}
