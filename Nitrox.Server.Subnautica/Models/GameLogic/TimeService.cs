using System.Collections.Generic;
using System.Diagnostics;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Networking;
using Nitrox.Server.Subnautica.Models.Events;
using Nitrox.Server.Subnautica.Services;
using Timer = System.Timers.Timer;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

internal sealed class TimeService : BackgroundService, ISummarize, IHibernate
{
    public delegate void TimeSkippedEventHandler(TimeSpan skippedTime);

    /// <summary>
    ///     Default time in Base SN is 480s
    /// </summary>
    public const int DEFAULT_STARTING_GAME_TIME_SECONDS = 480;

    /// <summary>
    ///     Time in seconds between each resync packet sending.
    /// </summary>
    /// <remarks>
    ///     AKA Interval of <see cref="resyncTimer" />.
    /// </remarks>
    private const int RESYNC_INTERVAL_SECONDS = 60;

    /// <summary>
    ///     Time in seconds between each ntp connection attempt.
    /// </summary>
    private const int NTP_RETRY_INTERVAL_SECONDS = 60;

    private readonly NtpSyncer ntpSyncer;
    private readonly ILogger<TimeService> logger;
    private readonly PlayerManager playerManager;

    private readonly PeriodicTimer resyncTimer = new(TimeSpan.FromSeconds(RESYNC_INTERVAL_SECONDS));
    private readonly Stopwatch stopWatch = new();

    /// <summary>
    ///     Latest registered time without taking the current stopwatch time in account.
    /// </summary>
    private double gameTimeMs;

    private double realTimeMs;

    public TimeSkippedEventHandler? TimeSkipped;

    /// <summary>
    ///     Total elapsed game time. Includes the real time while server is not sleeping. See
    ///     <see cref="HibernateService.IsSleeping" /> for more information.
    /// </summary>
    public TimeSpan GameTime
    {
        get => TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds + gameTimeMs);
        internal set
        {
            value = value == TimeSpan.Zero ? TimeSpan.FromSeconds(DEFAULT_STARTING_GAME_TIME_SECONDS) : value;
            gameTimeMs = value.TotalMilliseconds - stopWatch.ElapsedMilliseconds;
        }
    }

    public TimeSpan RealTime
    {
        get => TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds * 0.001 + realTimeMs);
        set => realTimeMs = value.TotalMilliseconds;
    }

    /// <summary>
    ///     Subnautica's equivalent of days.
    /// </summary>
    /// <remarks>
    ///     Uses ceiling because days count start at 1 and not 0.
    /// </remarks>
    public int GameDay => (int)Math.Ceiling(GameTime / TimeSpan.FromMinutes(20));

    public TimeService(PlayerManager playerManager, NtpSyncer ntpSyncer, ILogger<TimeService> logger)
    {
        this.playerManager = playerManager;
        this.ntpSyncer = ntpSyncer;
        this.logger = logger;

        // We only need the correction offset to be calculated once
        ntpSyncer.Setup(false, (onlineMode, _) =>
        {
            if (!onlineMode)
            {
                // until we get online even once, we'll retry the ntp sync sequence every NTP_RETRY_INTERVAL
                StartNtpTimer();
            }
        });
        ntpSyncer.RequestNtpService();
    }

    public void ResetCount()
    {
        stopWatch.Reset();
    }

    /// <summary>
    ///     Set current time depending on the current time in the day (replication of SN's system, see DayNightCycle.cs
    ///     commands for more information).
    /// </summary>
    /// <param name="type">Time to which you want to get to.</param>
    public void ChangeTime(StoryManager.TimeModification type)
    {
        TimeSpan skippedTime = TimeSpan.Zero;
        switch (type)
        {
            case StoryManager.TimeModification.DAY:
                skippedTime = TimeSpan.FromSeconds(1200 - GameTime.TotalSeconds % 1200 + 600);
                break;
            case StoryManager.TimeModification.NIGHT:
                skippedTime = TimeSpan.FromSeconds(1200 - GameTime.TotalSeconds % 1200);
                break;
            case StoryManager.TimeModification.SKIP:
                skippedTime = TimeSpan.FromSeconds(600 - GameTime.TotalSeconds % 600);
                break;
        }

        if (skippedTime.TotalSeconds > 0)
        {
            GameTime += skippedTime;
            TimeSkipped?.Invoke(skippedTime);

            playerManager.SendPacketToAllPlayers(MakeTimePacket());
        }
    }

    public TimeChange MakeTimePacket()
    {
        return new(GameTime.TotalSeconds, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), RealTime.TotalMilliseconds, ntpSyncer.OnlineMode, ntpSyncer.CorrectionOffset.Ticks);
    }

    public Task LogSummaryAsync(Perms viewerPerms)
    {
        logger.ZLogInformation($"Current time: day {GameDay} ({Math.Floor(GameTime.TotalSeconds)}s)");
        return Task.CompletedTask;
    }

    public Task SleepAsync()
    {
        stopWatch.Stop();
        resyncTimer.Period = TimeSpan.FromMicroseconds(uint.MaxValue); // uint.MaxValue removes internal .NET timer from ever ticking.
        return Task.CompletedTask;
    }

    public Task WakeAsync()
    {
        stopWatch.Start();
        resyncTimer.Period = TimeSpan.FromSeconds(RESYNC_INTERVAL_SECONDS);
        playerManager.SendPacketToAllPlayers(MakeTimePacket());
        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await resyncTimer.WaitForNextTickAsync(stoppingToken);
            playerManager.SendPacketToAllPlayers(MakeTimePacket());
        }
    }

    private void StartNtpTimer()
    {
        Timer retryTimer = new(TimeSpan.FromSeconds(NTP_RETRY_INTERVAL_SECONDS).TotalMilliseconds)
        {
            AutoReset = true,
        };
        retryTimer.Elapsed += delegate
        {
            // Reset the syncer before starting another round of it
            ntpSyncer.Dispose();
            ntpSyncer.Setup(false, (onlineMode, _) =>
            {
                if (onlineMode)
                {
                    retryTimer.Close();
                }
            });
            ntpSyncer.RequestNtpService();
        };
        retryTimer.Start();
    }
}
