using System;
using System.Diagnostics;
using System.Timers;
using NitroxModel.Networking;
using NitroxModel.Packets;
using static NitroxServer.GameLogic.StoryManager;

namespace NitroxServer.GameLogic;

public class TimeKeeper
{
    private readonly PlayerManager playerManager;
    private readonly NtpSyncer ntpSyncer;

    private readonly Stopwatch stopWatch = new();

    /// <summary>
    ///     Default time in Base SN is 480s
    /// </summary>
    public const int DEFAULT_TIME = 480;

    /// <summary>
    /// Latest registered time without taking the current stopwatch time in account.
    /// </summary>
    private double elapsedTimeOutsideStopWatchMs;

    private readonly double realTimeElapsed;

    /// <summary>
    /// Total elapsed time in milliseconds (adding the current stopwatch time with the latest registered time <see cref="elapsedTimeOutsideStopWatchMs"/>).
    /// </summary>
    public double ElapsedMilliseconds
    {
        get => stopWatch.ElapsedMilliseconds + elapsedTimeOutsideStopWatchMs;
        internal set
        {
            elapsedTimeOutsideStopWatchMs = value - stopWatch.ElapsedMilliseconds;
        }
    }

    /// <summary>
    /// Total elapsed time in seconds (converted from <see cref="ElapsedMilliseconds"/>).
    /// </summary>
    public double ElapsedSeconds
    {
        get => ElapsedMilliseconds * 0.001;
        set => ElapsedMilliseconds = value * 1000;
    }

    public double RealTimeElapsed => stopWatch.ElapsedMilliseconds * 0.001 + realTimeElapsed;

    /// <summary>
    /// Subnautica's equivalent of days.
    /// </summary>
    /// <remarks>
    /// Uses ceiling because days count start at 1 and not 0.
    /// </remarks>
    public int Day => (int)Math.Ceiling(ElapsedMilliseconds / TimeSpan.FromMinutes(20).TotalMilliseconds);

    /// <summary>
    /// Timer responsible for periodically sending time resync packets.
    /// </summary>
    /// <remarks>
    /// Is created by <see cref="MakeResyncTimer"/>.
    /// </remarks>
    public Timer ResyncTimer;
    /// <summary>
    /// Time in seconds between each resync packet sending.
    /// </summary>
    /// <remarks>
    /// AKA Interval of <see cref="ResyncTimer"/>.
    /// </remarks>
    private const int RESYNC_INTERVAL = 60;

    public TimeSkippedEventHandler TimeSkipped;

    /// <summary>
    /// Time in seconds between each ntp connection attempt.
    /// </summary>
    private const int NTP_RETRY_INTERVAL = 60;

    public TimeKeeper(PlayerManager playerManager, NtpSyncer ntpSyncer, double elapsedSeconds, double realTimeElapsed)
    {
        this.playerManager = playerManager;
        this.ntpSyncer = ntpSyncer;

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

        elapsedTimeOutsideStopWatchMs = elapsedSeconds == 0 ? TimeSpan.FromSeconds(DEFAULT_TIME).TotalMilliseconds : elapsedSeconds * 1000;
        this.realTimeElapsed = realTimeElapsed;
        ResyncTimer = MakeResyncTimer();
    }

    /// <summary>
    /// Creates a timer that periodically sends resync packets to players.
    /// </summary>
    public Timer MakeResyncTimer()
    {
        Timer resyncTimer = new()
        {
            Interval = TimeSpan.FromSeconds(RESYNC_INTERVAL).TotalMilliseconds,
            AutoReset = true
        };
        resyncTimer.Elapsed += delegate
        {
            playerManager.SendPacketToAllPlayers(MakeTimePacket());
        };
        return resyncTimer;
    }

    private void StartNtpTimer()
    {
        Timer retryTimer = new(TimeSpan.FromSeconds(NTP_RETRY_INTERVAL).TotalMilliseconds)
        {
            AutoReset = true,
        };
        
        retryTimer.Elapsed += delegate
        {
            // Reset the syncer before starting another round of it
            ntpSyncer.Dispose();
            ntpSyncer.Setup(true, (onlineMode, _) =>  // TODO: set to false after tests
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

    public void StartCounting()
    {
        stopWatch.Start();
        ResyncTimer.Start();
        playerManager.SendPacketToAllPlayers(MakeTimePacket());
    }

    public void ResetCount()
    {
        stopWatch.Reset();
    }

    public void StopCounting()
    {
        stopWatch.Stop();
        ResyncTimer.Stop();
    }

    /// <summary>
    /// Set current time depending on the current time in the day (replication of SN's system, see DayNightCycle.cs commands for more information).
    /// </summary>
    /// <param name="type">Time to which you want to get to.</param>
    public void ChangeTime(TimeModification type)
    {
        double skipAmount = 0;
        switch (type)
        {
            case TimeModification.DAY:
                skipAmount = 1200 - (ElapsedSeconds % 1200) + 600;
                break;
            case TimeModification.NIGHT:
                skipAmount = 1200 - (ElapsedSeconds % 1200);
                break;
            case TimeModification.SKIP:
                skipAmount = 600 - (ElapsedSeconds % 600);
                break;
        }
        
        if (skipAmount > 0)
        {
            ElapsedSeconds += skipAmount;
            TimeSkipped?.Invoke(skipAmount);

            playerManager.SendPacketToAllPlayers(MakeTimePacket());
        }
    }

    public TimeChange MakeTimePacket()
    {
        return new(ElapsedSeconds, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), RealTimeElapsed, ntpSyncer.OnlineMode, ntpSyncer.CorrectionOffset.Ticks);
    }

    public delegate void TimeSkippedEventHandler(double skipAmount);
}
