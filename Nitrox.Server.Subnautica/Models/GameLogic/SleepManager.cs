using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.AppEvents;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Timer = System.Timers.Timer;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

internal sealed class SleepManager(IPacketSender packetSender, PlayerManager playerManager, TimeService timeService) : ISessionCleaner
{
    /// <summary>Duration of the sleep animation/screen fade in seconds.</summary>
    private const float SLEEP_DURATION = 5f;
    /// <summary>Time to skip when sleeping. From Bed.kSleepEndTime - Bed.kSleepStartTime (1188 - 792 = 396).</summary>
    private const float SLEEP_TIME_SKIP_SECONDS = 396f;

    private readonly IPacketSender packetSender = packetSender;
    private readonly TimeService timeService = timeService;
    private readonly ThreadSafeSet<SessionId> playerIdsInBed = [];
    private bool isSleepInProgress;
    private Timer? sleepTimer;
    private readonly PlayerManager playerManager = playerManager;

    public void PlayerEnteredBed(Player player)
    {
        if (!playerIdsInBed.Add(player.SessionId))
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
        if (!playerIdsInBed.Remove(player.SessionId))
        {
            return;
        }

        BroadcastStatus();
    }

    private bool AreAllPlayersInBed()
    {
        int totalPlayers = playerManager.GetConnectedPlayers().Count;
        return totalPlayers > 0 && playerIdsInBed.Count >= totalPlayers;
    }

    private void BroadcastStatus()
    {
        int totalPlayers = playerManager.GetConnectedPlayers().Count;
        packetSender.SendPacketToAllAsync(new SleepStatusUpdate(playerIdsInBed.Count, totalPlayers));
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
            packetSender.SendPacketToAllAsync(new SleepComplete());
            isSleepInProgress = false;
            sleepTimer.Dispose();
            sleepTimer = null;
        };
        sleepTimer?.Start();

        playerIdsInBed.Clear();
    }

    public Task OnEventAsync(ISessionCleaner.Args args)
    {
        playerIdsInBed.Remove(args.Session.Id);
        // If sleep is already in progress, let it complete - don't cancel just because someone disconnected
        if (isSleepInProgress)
        {
            return Task.CompletedTask;
        }
        if (playerIdsInBed.Count <= 0)
        {
            return Task.CompletedTask;
        }

        // Send to all players except the disconnecting one
        SleepStatusUpdate packet = new(playerIdsInBed.Count, args.NewPlayerTotal);
        packetSender.SendPacketToOthersAsync(packet, args.Session.Id);

        // Check if remaining players are now all sleeping (disconnected player was the only one awake)
        if (args.NewPlayerTotal > 0 && playerIdsInBed.Count >= args.NewPlayerTotal)
        {
            StartSleep();
        }
        return Task.CompletedTask;
    }
}
