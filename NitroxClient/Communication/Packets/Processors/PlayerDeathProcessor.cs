﻿using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.Helper;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerDeathProcessor : ClientPacketProcessor<PlayerDeathEvent>
{
    private readonly PlayerManager playerManager;

    public PlayerDeathProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public override void Process(PlayerDeathEvent playerDeath)
    {
        RemotePlayer player = Validate.IsPresent(playerManager.Find(playerDeath.PlayerId));
        Log.Debug($"{player.PlayerName} died");
        Log.InGame(Language.main.Get("Nitrox_PlayerDied").Replace("{PLAYER}", player.PlayerName));
        player.PlayerDeathEvent.Trigger(player);

        // TODO: Add any death related triggers (i.e. scoreboard updates, rewards, etc.)
    }
}
