using System;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Nitrox.Server.Subnautica.Models.Hibernation;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using NitroxModel.Networking.Packets;
using NitroxServer.Helper;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Keeps track of (game) time.
/// </summary>
internal sealed class TimeService(IServerPacketSender packetSender, NtpSyncer ntpSyncer) : IHostedService, IHibernate
{
    public delegate void TimeSkippedEventHandler(double skippedSeconds);

    /// <summary>
    ///     Time in seconds between each resync packet sending.
    /// </summary>
    /// <remarks>
    ///     AKA Interval of <see cref="ResyncTimer" />.
    /// </remarks>
    private const int RESYNC_INTERVAL = 60;

    /// <summary>
    ///     Time in seconds between each ntp connection attempt.
    /// </summary>
    private const int NTP_RETRY_INTERVAL = 60;

    private readonly NtpSyncer ntpSyncer = ntpSyncer;

    private readonly Stopwatch stopWatch = new();

    /// <summary>
    ///     Timer responsible for periodically sending time resync packets.
    /// </summary>
    /// <remarks>
    ///     Is created by <see cref="MakeResyncTimer" />.
    /// </remarks>
    public System.Timers.Timer ResyncTimer;

    public TimeSkippedEventHandler TimeSkipped;
    private readonly IServerPacketSender packetSender = packetSender;

    /// <summary>
    ///     Total elapsed time (adding the current stopwatch time with the latest registered time).
    /// </summary>
    public TimeSpan Elapsed
    {
        get => stopWatch.Elapsed;
        set
        {
            // TODO: USE DATABASE
        }
        // TODO: USE DATABASE
        // get => stopWatch.Elapsed + stateProvider.State.Elapsed;
        // private set => stateProvider.State.Elapsed = value - stopWatch.Elapsed;
    }

    public TimeSpan RealTimeElapsed => stopWatch.Elapsed + Elapsed;

    /// <summary>
    ///     Subnautica's equivalent of days.
    /// </summary>
    /// <remarks>
    ///     Uses ceiling because days count start at 1 and not 0.
    /// </remarks>
    public int Day => (int)Math.Ceiling(Elapsed / TimeSpan.FromMinutes(20));

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // We only need the correction offset to be calculated once
        ntpSyncer.Setup(true, (onlineMode, _) => // TODO: set to false after tests
        {
            if (!onlineMode)
            {
                // until we get online even once, we'll retry the ntp sync sequence every NTP_RETRY_INTERVAL
                StartNtpTimer();
            }
        });
        ntpSyncer.RequestNtpService();
        ResyncTimer = MakeResyncTimer();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    ///     Creates a timer that periodically sends resync packets to players.
    /// </summary>
    public System.Timers.Timer MakeResyncTimer()
    {
        System.Timers.Timer resyncTimer = new()
        {
            Interval = TimeSpan.FromSeconds(RESYNC_INTERVAL).TotalMilliseconds,
            AutoReset = true
        };
        resyncTimer.Elapsed += delegate
        {
            packetSender.SendPacketToAll(MakeTimePacket());
        };
        return resyncTimer;
    }

    public void ResetCount() => stopWatch.Reset();

    /// <summary>
    ///     Set current time depending on the current time in the day (replication of SN's system, see DayNightCycle.cs
    ///     commands for more information).
    /// </summary>
    /// <param name="type">Time to which you want to get to.</param>
    public void ChangeTime(StoryTimingService.TimeModification type)
    {
        double skipAmount = 0;
        switch (type)
        {
            case StoryTimingService.TimeModification.DAY:
                skipAmount = 1200 - Elapsed.TotalSeconds % 1200 + 600;
                break;
            case StoryTimingService.TimeModification.NIGHT:
                skipAmount = 1200 - Elapsed.TotalSeconds % 1200;
                break;
            case StoryTimingService.TimeModification.SKIP:
                skipAmount = 600 - Elapsed.TotalSeconds % 600;
                break;
        }

        if (skipAmount > 0)
        {
            Elapsed += TimeSpan.FromSeconds(skipAmount);
            TimeSkipped?.Invoke(skipAmount);

            packetSender.SendPacketToAll(MakeTimePacket());
        }
    }

    public TimeChange MakeTimePacket() => new(Elapsed.TotalSeconds, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), RealTimeElapsed.TotalSeconds, ntpSyncer.OnlineMode, ntpSyncer.CorrectionOffset.Ticks);

    /// <summary>
    ///     Calculate the time at Aurora's explosion countdown will begin.
    /// </summary>
    /// <remarks>
    ///     Takes the current time into account.
    /// </remarks>
    private double GenerateDeterministicAuroraTime(string seed)
    {
        // Copied from CrashedShipExploder.SetExplodeTime() and changed from seconds to ms
        DeterministicGenerator generator = new(seed, nameof(StoryTimingService));
        return Elapsed.TotalMilliseconds + generator.NextDouble(2.3d, 4d) * 1200d * 1000d;
    }

    private void StartNtpTimer()
    {
        System.Timers.Timer retryTimer = new(TimeSpan.FromSeconds(NTP_RETRY_INTERVAL).TotalMilliseconds) { AutoReset = true };
        retryTimer.Elapsed += delegate
        {
            // Reset the syncer before starting another round of it
            ntpSyncer.Dispose();
            ntpSyncer.Setup(true, (onlineMode, _) => // TODO: set to false after tests
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

    public void Hibernate()
    {
        stopWatch.Stop();
        ResyncTimer.Stop();
    }

    public void Resume()
    {
        stopWatch.Start();
        ResyncTimer.Start();
        packetSender.SendPacketToAll(MakeTimePacket());
    }
}
