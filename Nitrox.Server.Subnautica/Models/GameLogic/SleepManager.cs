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
    private readonly ThreadSafeSet<SessionId> sessionIdsInBed = [];
    private bool isSleepInProgress;
    private Timer? sleepTimer;
    private readonly PlayerManager playerManager = playerManager;

    public void PlayerEnteredBed(Player player)
    {
        if (!sessionIdsInBed.Add(player.SessionId))
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
        if (!sessionIdsInBed.Remove(player.SessionId))
        {
            return;
        }

        BroadcastStatus();
    }

    private bool AreAllPlayersInBed()
    {
        int totalPlayers = playerManager.GetConnectedPlayers().Count;
        return totalPlayers > 0 && sessionIdsInBed.Count >= totalPlayers;
    }

    private void BroadcastStatus()
    {
        int totalPlayers = playerManager.GetConnectedPlayers().Count;
        packetSender.SendPacketToAllAsync(new SleepStatusUpdate(sessionIdsInBed.Count, totalPlayers));
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

        sessionIdsInBed.Clear();
    }

    public async Task OnEventAsync(ISessionCleaner.Args args)
    {
        sessionIdsInBed.Remove(args.Session.Id);
        // If sleep is already in progress, let it complete - don't cancel just because someone disconnected
        if (isSleepInProgress)
        {
            return;
        }
        if (sessionIdsInBed.Count <= 0)
        {
            return;
        }

        // Send to all players except the disconnecting one
        SleepStatusUpdate packet = new(sessionIdsInBed.Count, args.NewPlayerTotal);
        await packetSender.SendPacketToOthersAsync(packet, args.Session.Id);

        // Check if remaining players are now all sleeping (disconnected player was the only one awake)
        if (args.NewPlayerTotal > 0 && sessionIdsInBed.Count >= args.NewPlayerTotal)
        {
            StartSleep();
        }
    }
}
