using System;
using Nitrox.Model.DataStructures;
using Timer = System.Timers.Timer;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

internal sealed class SleepManager(PlayerManager playerManager, TimeService timeService)
{
    /// <summary>Duration of the sleep animation/screen fade in seconds.</summary>
    private const float SLEEP_DURATION = 5f;
    /// <summary>Time to skip when sleeping. From Bed.kSleepEndTime - Bed.kSleepStartTime (1188 - 792 = 396).</summary>
    private const float SLEEP_TIME_SKIP_SECONDS = 396f;

    private readonly PlayerManager playerManager = playerManager;
    private readonly TimeService timeService = timeService;
    private readonly ThreadSafeSet<ushort> playerIdsInBed = [];
    private bool isSleepInProgress;
    private Timer? sleepTimer;

    public void PlayerEnteredBed(Player player)
    {
        if (!playerIdsInBed.Add(player.Id))
        {
            return;
        }

        BroadcastStatus();
        if (!isSleepInProgress && AreAllPlayersInBed())
        {
            StartSleep();
        }
    }

    public void PlayerExitedBed(Player player)
    {
        if (!playerIdsInBed.Remove(player.Id))
        {
            return;
        }

        BroadcastStatus();
    }

    public void PlayerDisconnected(Player player)
    {
        playerIdsInBed.Remove(player.Id);
        // If sleep is already in progress, let it complete - don't cancel just because someone disconnected
        if (isSleepInProgress)
        {
            return;
        }
        if (playerIdsInBed.Count <= 0)
        {
            return;
        }

        // Note: The disconnecting player is still in playerManager at this point, so we subtract 1
        int remainingPlayers = playerManager.GetConnectedPlayers().Count - 1;
            
        // Send to all players except the disconnecting one
        SleepStatusUpdate packet = new(playerIdsInBed.Count, remainingPlayers);
        foreach (Player connectedPlayer in playerManager.GetConnectedPlayers())
        {
            if (connectedPlayer.Id != player.Id)
            {
                connectedPlayer.SendPacket(packet);
            }
        }

        // Check if remaining players are now all sleeping (disconnected player was the only one awake)
        if (remainingPlayers > 0 && playerIdsInBed.Count >= remainingPlayers)
        {
            StartSleep();
        }
    }

    private bool AreAllPlayersInBed()
    {
        int totalPlayers = playerManager.GetConnectedPlayers().Count;
        return totalPlayers > 0 && playerIdsInBed.Count >= totalPlayers;
    }

    private void BroadcastStatus()
    {
        int totalPlayers = playerManager.GetConnectedPlayers().Count;
        playerManager.SendPacketToAllPlayers(new SleepStatusUpdate(playerIdsInBed.Count, totalPlayers));
    }

    private void StartSleep()
    {
        isSleepInProgress = true;

        sleepTimer = new Timer(TimeSpan.FromSeconds(SLEEP_DURATION).TotalMilliseconds)
        {
            AutoReset = false
        };
        sleepTimer.Elapsed += delegate
        {
            timeService.SkipTime(TimeSpan.FromSeconds(SLEEP_TIME_SKIP_SECONDS));
            playerManager.SendPacketToAllPlayers(new SleepComplete());
            isSleepInProgress = false;
            sleepTimer.Dispose();
            sleepTimer = null;
        };
        sleepTimer?.Start();

        playerIdsInBed.Clear();
    }
}
