using System.Diagnostics;
using Nitrox.Model.Networking;
using Nitrox.Server.Subnautica.Models.AppEvents;
using Nitrox.Server.Subnautica.Models.AppEvents.Core;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Services;
using Timer = System.Timers.Timer;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

internal sealed class TimeService(IPacketSender packetSender, NtpSyncer ntpSyncer, ILoggerFactory loggerFactory, ILogger<TimeService> logger)
    : BackgroundService, ISummarize, IHibernate
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

    /// <summary>
    ///     Number of consecutive failed retry rounds before the circuit breaker opens and we assume there is
    ///     no NTP connectivity (e.g. an offline/LAN server).
    /// </summary>
    private const int NTP_FAILURE_THRESHOLD = 3;

    /// <summary>
    ///     Retry interval in seconds used once the circuit breaker has opened. Slows futile attempts right down
    ///     while still acting as a half-open probe so the server recovers if it later gains connectivity.
    /// </summary>
    private const int NTP_OFFLINE_RETRY_INTERVAL_SECONDS = 1800;

    private readonly ILogger<TimeService> logger = logger;
    private readonly ILoggerFactory loggerFactory = loggerFactory;

    private readonly NtpSyncer ntpSyncer = ntpSyncer;
    private readonly IPacketSender packetSender = packetSender;

    private readonly PeriodicTimer resyncTimer = new(TimeSpan.FromSeconds(RESYNC_INTERVAL_SECONDS));
    private readonly Stopwatch stopWatch = new();

    private double activeRealTimeSeconds;

    public TimeSkippedEventHandler? TimeSkipped;

    /// <summary>
    ///     Gets the total game time the server was actively simulating the game. See
    ///     <see cref="HibernateService.IsSleeping" /> for more information.
    /// </summary>
    /// <remarks>
    ///     Initial value is <see cref="DEFAULT_STARTING_GAME_TIME_SECONDS" /> for fresh worlds.
    /// </remarks>
    public TimeSpan GameTime
    {
        get => ActiveTime + TimeSpan.FromSeconds(DEFAULT_STARTING_GAME_TIME_SECONDS);
        internal set
        {
            value -= TimeSpan.FromSeconds(DEFAULT_STARTING_GAME_TIME_SECONDS);
            activeRealTimeSeconds = double.Max(0, value.TotalSeconds - stopWatch.Elapsed.TotalSeconds);
        }
    }

    /// <summary>
    ///     Gets the total time the server was actively simulating the game. This excludes time during
    ///     <see cref="HibernateService.IsSleeping" />.
    /// </summary>
    /// <remarks>
    ///     Initial value is 0 for fresh worlds.
    /// </remarks>
    public TimeSpan ActiveTime
    {
        get => TimeSpan.FromSeconds(activeRealTimeSeconds + stopWatch.Elapsed.TotalSeconds);
        set => activeRealTimeSeconds = double.Max(0, value.TotalSeconds);
    }

    /// <summary>
    ///     Subnautica's equivalent of days.
    /// </summary>
    /// <remarks>
    ///     Uses ceiling because days count start at 1 and not 0.
    /// </remarks>
    public int GameDay => (int)Math.Ceiling(GameTime / TimeSpan.FromMinutes(20));

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

            packetSender.SendPacketToAllAsync(MakeTimePacket());
        }
    }

    public TimeChange MakeTimePacket()
    {
        return new(GameTime.TotalSeconds, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), ActiveTime.TotalSeconds, ntpSyncer.OnlineMode, ntpSyncer.CorrectionOffset.Ticks);
    }

    /// <summary>
    ///     Skips time by the specified amount and broadcasts the update to all players.
    /// </summary>
    public void SkipTime(TimeSpan skipAmount)
    {
        if (skipAmount <= TimeSpan.Zero)
        {
            return;
        }

        GameTime += skipAmount;
        TimeSkipped?.Invoke(skipAmount);
        packetSender.SendPacketToAllAsync(MakeTimePacket());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // We only need the correction offset to be calculated once
        ntpSyncer.Setup((onlineMode, _) =>
        {
            if (!onlineMode)
            {
                // until we get online even once, we'll retry the ntp sync sequence every NTP_RETRY_INTERVAL
                StartNtpTimer();
            }
        }, loggerFactory.CreateLogger<NtpSyncer>());
        ntpSyncer.RequestNtpService();

        while (!stoppingToken.IsCancellationRequested)
        {
            await resyncTimer.WaitForNextTickAsync(stoppingToken);
            await packetSender.SendPacketToAllAsync(MakeTimePacket());
        }
    }

    Task IEvent<IHibernate.SleepArgs>.OnEventAsync(IHibernate.SleepArgs args)
    {
        stopWatch.Stop();
        resyncTimer.Period = TimeSpan.FromMicroseconds(uint.MaxValue); // uint.MaxValue removes internal .NET timer from ever ticking.
        return Task.CompletedTask;
    }

    async Task IEvent<IHibernate.WakeArgs>.OnEventAsync(IHibernate.WakeArgs args)
    {
        stopWatch.Start();
        resyncTimer.Period = TimeSpan.FromSeconds(RESYNC_INTERVAL_SECONDS);
        await packetSender.SendPacketToAllAsync(MakeTimePacket());
    }

    Task IEvent<ISummarize.Args>.OnEventAsync(ISummarize.Args args)
    {
        logger.ZLogInformation($"Current time: day {GameDay} ({Math.Floor(GameTime.TotalSeconds)}s)");
        return Task.CompletedTask;
    }

    private void StartNtpTimer()
    {
        CircuitBreaker circuitBreaker = new(NTP_FAILURE_THRESHOLD);
        Timer retryTimer = new(TimeSpan.FromSeconds(NTP_RETRY_INTERVAL_SECONDS).TotalMilliseconds)
        {
            AutoReset = true,
        };
        retryTimer.Elapsed += delegate
        {
            // Reset the syncer before starting another round of it
            ntpSyncer.Complete();
            ntpSyncer.Setup((onlineMode, _) =>
            {
                if (onlineMode)
                {
                    circuitBreaker.RecordSuccess();
                    retryTimer.Close();
                    return;
                }

                // After enough consecutive failures, assume there's no NTP connectivity (e.g. an offline/LAN
                // server) and slow the retries right down so we stop wasting resources and log lines.
                if (circuitBreaker.RecordFailure())
                {
                    retryTimer.Interval = TimeSpan.FromSeconds(NTP_OFFLINE_RETRY_INTERVAL_SECONDS).TotalMilliseconds;
                    logger.ZLogInformation($"Could not reach any NTP server after {NTP_FAILURE_THRESHOLD} attempts; assuming no internet connection and slowing NTP retries to every {NTP_OFFLINE_RETRY_INTERVAL_SECONDS}s. This is expected on offline/LAN servers.");
                }
            }, loggerFactory.CreateLogger<NtpSyncer>());
            ntpSyncer.RequestNtpService();
        };
        retryTimer.Start();
    }
}
