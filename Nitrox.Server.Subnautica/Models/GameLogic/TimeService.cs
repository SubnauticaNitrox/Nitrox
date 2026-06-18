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
    private const int NTP_RETRY_BREAKER_INTERVAL_MINUTES = 10;
    private const int NTP_RETRY_BREAKER_FAILURE_THRESHOLD = 3;

    private readonly ILogger<TimeService> logger = logger;
    private readonly ILoggerFactory loggerFactory = loggerFactory;
    private readonly NtpRetryCircuitBreaker ntpRetryCircuitBreaker = new(
        TimeSpan.FromSeconds(NTP_RETRY_INTERVAL_SECONDS),
        TimeSpan.FromMinutes(NTP_RETRY_BREAKER_INTERVAL_MINUTES),
        NTP_RETRY_BREAKER_FAILURE_THRESHOLD);

    private readonly NtpSyncer ntpSyncer = ntpSyncer;
    private readonly IPacketSender packetSender = packetSender;

    private readonly PeriodicTimer resyncTimer = new(TimeSpan.FromSeconds(RESYNC_INTERVAL_SECONDS));
    private readonly Stopwatch stopWatch = new();
    private readonly Lock ntpRetryTimerLock = new();

    private double activeRealTimeSeconds;
    private Timer? ntpRetryTimer;

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
        ntpSyncer.Setup(OnNtpSyncCompleted, loggerFactory.CreateLogger<NtpSyncer>());
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

    private void OnNtpSyncCompleted(bool onlineMode, TimeSpan _)
    {
        if (onlineMode)
        {
            ntpRetryCircuitBreaker.RegisterSuccess();
            CloseNtpRetryTimer();
            return;
        }

        RetryDecision retryDecision = ntpRetryCircuitBreaker.RegisterFailure();
        if (retryDecision.BreakerOpened)
        {
            logger.ZLogWarning($"NTP sync failed {NTP_RETRY_BREAKER_FAILURE_THRESHOLD} times in a row; delaying the next retry for {retryDecision.Delay.TotalMinutes:0} minutes.");
        }

        StartNtpTimer(retryDecision.Delay);
    }

    private void StartNtpTimer(TimeSpan retryDelay)
    {
        CloseNtpRetryTimer();

        Timer retryTimer = new(retryDelay.TotalMilliseconds)
        {
            AutoReset = false,
        };
        retryTimer.Elapsed += delegate
        {
            ntpSyncer.Complete();
            ntpSyncer.Setup(OnNtpSyncCompleted, loggerFactory.CreateLogger<NtpSyncer>());
            ntpSyncer.RequestNtpService();
        };

        lock (ntpRetryTimerLock)
        {
            ntpRetryTimer = retryTimer;
        }

        retryTimer.Start();
    }

    private void CloseNtpRetryTimer()
    {
        lock (ntpRetryTimerLock)
        {
            ntpRetryTimer?.Close();
            ntpRetryTimer = null;
        }
    }
}
