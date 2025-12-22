using System;
using System.Collections.Generic;
using Timer = System.Timers.Timer;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

public class SleepManager
{
    /// <summary>Duration of the sleep animation/screen fade in seconds.</summary>
    private const float SLEEP_DURATION = 5f;
    /// <summary>Time to skip when sleeping. From Bed.kSleepEndTime - Bed.kSleepStartTime (1188 - 792 = 396).</summary>
    private const float SLEEP_TIME_SKIP = 396f;

    private readonly PlayerManager playerManager;
    private readonly TimeKeeper timeKeeper;
    private readonly HashSet<ushort> sleepingPlayerIds = [];
    private bool isSleepInProgress;
    private Timer? sleepTimer;

    public SleepManager(PlayerManager playerManager, TimeKeeper timeKeeper)
    {
        this.playerManager = playerManager;
        this.timeKeeper = timeKeeper;
    }

    public void PlayerEnteredBed(Player player)
    {
        if (!sleepingPlayerIds.Add(player.Id))
        {
            return;
        }

        BroadcastSleepStatus();

        if (!isSleepInProgress && AreAllPlayersSleeping())
        {
            StartSleep();
        }
    }

    public void PlayerExitedBed(Player player)
    {
        if (!sleepingPlayerIds.Remove(player.Id))
        {
            return;
        }

        bool wasCancelled = isSleepInProgress;
        if (wasCancelled)
        {
            CancelSleep();
        }

        BroadcastSleepStatus(wasCancelled);
    }

    public void PlayerDisconnected(Player player)
    {
        sleepingPlayerIds.Remove(player.Id);

        // If sleep is already in progress, let it complete - don't cancel just because someone disconnected
        if (isSleepInProgress)
        {
            return;
        }

        if (sleepingPlayerIds.Count > 0)
        {
            // Note: The disconnecting player is still in playerManager at this point, so we subtract 1
            int remainingPlayers = playerManager.GetConnectedPlayers().Count - 1;
            
            // Send to all players except the disconnecting one
            SleepStatusUpdate packet = new(sleepingPlayerIds.Count, remainingPlayers, false);
            foreach (Player connectedPlayer in playerManager.GetConnectedPlayers())
            {
                if (connectedPlayer.Id != player.Id)
                {
                    connectedPlayer.SendPacket(packet);
                }
            }

            // Check if remaining players are now all sleeping (disconnected player was the only one awake)
            if (remainingPlayers > 0 && sleepingPlayerIds.Count >= remainingPlayers)
            {
                StartSleep();
            }
        }
    }

    private bool AreAllPlayersSleeping()
    {
        int totalPlayers = playerManager.GetConnectedPlayers().Count;
        return totalPlayers > 0 && sleepingPlayerIds.Count >= totalPlayers;
    }

    private void BroadcastSleepStatus(bool wasCancelled = false)
    {
        int totalPlayers = playerManager.GetConnectedPlayers().Count;
        int sleepingCount = sleepingPlayerIds.Count;

        playerManager.SendPacketToAllPlayers(new SleepStatusUpdate(sleepingCount, totalPlayers, wasCancelled));
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
            timeKeeper.SkipTime(SLEEP_TIME_SKIP);
            playerManager.SendPacketToAllPlayers(new SleepComplete());
            isSleepInProgress = false;
            sleepTimer.Dispose();
            sleepTimer = null;
        };
        sleepTimer?.Start();

        sleepingPlayerIds.Clear();
    }

    private void CancelSleep()
    {
        if (sleepTimer != null)
        {
            sleepTimer.Stop();
            sleepTimer.Dispose();
            sleepTimer = null;
        }
        isSleepInProgress = false;
    }
}
