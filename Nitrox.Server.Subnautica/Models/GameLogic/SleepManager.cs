using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.AppEvents;
using Nitrox.Server.Subnautica.Models.AppEvents.Core;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

internal sealed class SleepManager(IPacketSender packetSender, PlayerService playerService, TimeService timeService) : ISessionCleaner
{
    /// <summary>Duration of the sleep animation/screen fade in seconds.</summary>
    private const float SLEEP_DURATION = 5f;

    /// <summary>Time to skip when sleeping. From Bed.kSleepEndTime - Bed.kSleepStartTime (1188 - 792 = 396).</summary>
    private const float SLEEP_TIME_SKIP_SECONDS = 396f;

    private readonly IPacketSender packetSender = packetSender;
    private readonly PlayerService playerService = playerService;
    private readonly ThreadSafeSet<SessionId> sessionIdsInBed = [];
    private readonly TimeService timeService = timeService;
    private bool isSleepInProgress;

    public async Task PlayerEnteredBed(SessionId player)
    {
        if (!sessionIdsInBed.Add(player))
        {
            return;
        }

        await BroadcastStatus();
        if (!isSleepInProgress && AreAllPlayersInBed())
        {
            await StartSleep();
        }
    }

    public async Task PlayerExitedBed(SessionId player)
    {
        if (!sessionIdsInBed.Remove(player))
        {
            return;
        }

        await BroadcastStatus();
    }

    private bool AreAllPlayersInBed()
    {
        int totalPlayers = playerService.ConnectedPlayerCount;
        return totalPlayers > 0 && sessionIdsInBed.Count >= totalPlayers;
    }

    private async Task BroadcastStatus()
    {
        int totalPlayers = playerService.ConnectedPlayerCount;
        await packetSender.SendPacketToAllAsync(new SleepStatusUpdate(sessionIdsInBed.Count, totalPlayers));
    }

    private async Task StartSleep()
    {
        isSleepInProgress = true;
        sessionIdsInBed.Clear();

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(SLEEP_DURATION));
            await timeService.SkipTimeAsync(TimeSpan.FromSeconds(SLEEP_TIME_SKIP_SECONDS));
            await packetSender.SendPacketToAllAsync(new SleepComplete());
        }
        finally
        {
            isSleepInProgress = false;
        }
    }

    async Task IEvent<ISessionCleaner.Args>.OnEventAsync(ISessionCleaner.Args args)
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
        SleepStatusUpdate packet = new(sessionIdsInBed.Count, args.NewSessionTotal);
        await packetSender.SendPacketToOthersAsync(packet, args.Session.Id);

        // Check if remaining players are now all sleeping (disconnected player was the only one awake)
        if (args.NewSessionTotal > 0 && sessionIdsInBed.Count >= args.NewSessionTotal)
        {
            await StartSleep();
        }
    }
}
