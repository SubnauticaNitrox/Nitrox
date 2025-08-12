using System.Collections;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;
using UWE;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerJoinedMultiplayerSessionProcessor : ClientPacketProcessor<PlayerJoinedMultiplayerSession>
{
    private readonly PlayerManager playerManager;
    private readonly Entities entities;

    public PlayerJoinedMultiplayerSessionProcessor(PlayerManager playerManager, Entities entities)
    {
        this.playerManager = playerManager;
        this.entities = entities;
    }

    public override void Process(PlayerJoinedMultiplayerSession packet)
    {
        CoroutineHost.StartCoroutine(SpawnRemotePlayer(packet));
    }

    private IEnumerator SpawnRemotePlayer(PlayerJoinedMultiplayerSession packet)
    {
        playerManager.Create(packet.PlayerContext);
        yield return entities.SpawnEntityAsync(packet.PlayerWorldEntity, true, true);

        Log.Info($"{packet.PlayerContext.PlayerName} joined the game");
        Log.InGame(Language.main.Get("Nitrox_PlayerJoined").Replace("{PLAYER}", packet.PlayerContext.PlayerName));
    }
}
