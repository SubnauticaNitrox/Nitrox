using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Helper;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.GameLogic;

/// <summary>
/// Keeps track of Aurora-related events
/// </summary>
public class StoryManager
{
    private readonly Stopwatch stopWatch = new();
    private readonly PlayerManager playerManager;
    private readonly PDAStateData pdaStateData;
    private readonly StoryGoalData storyGoalData;
    private string seed;

    public double AuroraExplosionTimeMs;
    public double AuroraWarningTimeMs;

    private static readonly List<string> auroraEvents = new() { "Story_AuroraWarning1", "Story_AuroraWarning2", "Story_AuroraWarning3", "Story_AuroraWarning4", "Story_AuroraExplosion" };

    private double elapsedTimeOutsideStopWatchMs;

    /// <summary>
    /// Total Elapsed Time in milliseconds
    /// </summary>
    public double ElapsedTimeMs
    {
        get => stopWatch.ElapsedMilliseconds + elapsedTimeOutsideStopWatchMs;
        internal set
        {
            elapsedTimeOutsideStopWatchMs = value - stopWatch.ElapsedMilliseconds;
        }
    }

    /// <summary>
    /// Total Elapsed Time in seconds
    /// </summary>
    public double ElapsedSeconds
    {
        get => ElapsedTimeMs * 0.001;
        private set => ElapsedTimeMs = value * 1000;
    }

    /// <summary>
    /// Subnautica's equivalent of days
    /// </summary>
    // Using ceiling because days count start at 1 and not 0
    public int Day => (int)Math.Ceiling(ElapsedTimeMs / TimeSpan.FromMinutes(20).TotalMilliseconds);

    public StoryManager(PlayerManager playerManager, PDAStateData pdaStateData, StoryGoalData storyGoalData, string seed, double elapsedSeconds, double? auroraExplosionTime, double? auroraWarningTime)
    {
        this.playerManager = playerManager;
        this.pdaStateData = pdaStateData;
        this.storyGoalData = storyGoalData;
        this.seed = seed;
        // Default time in Base SN is 480s
        elapsedTimeOutsideStopWatchMs = elapsedSeconds == 0 ? TimeSpan.FromSeconds(480).TotalMilliseconds : elapsedSeconds * 1000;
        AuroraExplosionTimeMs = auroraExplosionTime ?? GenerateDeterministicAuroraTime(seed);
        AuroraWarningTimeMs = auroraWarningTime ?? ElapsedTimeMs;
    }
    /// <summary>
    /// Tells the players to start Aurora's explosion event
    /// </summary>
    /// <param name="cooldown">Wether we should make Aurora explode instantly or after a short countdown</param>
    public void ExplodeAurora(bool cooldown)
    {
        AuroraExplosionTimeMs = ElapsedTimeMs;
        // Explode aurora with a cooldown is like default game just before aurora is about to explode
        if (cooldown)
        {
            // These lines should be filled with the same informations as in the constructor
            playerManager.SendPacketToAllPlayers(new StoryGoalExecuted("Story_AuroraWarning4", StoryGoalExecuted.EventType.PDA_EXTRA, (float)ElapsedSeconds));
            playerManager.SendPacketToAllPlayers(new StoryGoalExecuted("Story_AuroraExplosion", StoryGoalExecuted.EventType.EXTRA, (float)ElapsedSeconds));
            Log.Info("Started Aurora's explosion sequence");
        }
        else
        {
            // This will make aurora explode instantly on clients
            playerManager.SendPacketToAllPlayers(new AuroraExplodeNow());
            Log.Info("Exploded Aurora");
        }
    }

    /// <summary>
    /// Tells the players to start Aurora's restoration event
    /// </summary>
    public void RestoreAurora()
    {
        AuroraExplosionTimeMs = GenerateDeterministicAuroraTime(seed) + ElapsedTimeMs;
        AuroraWarningTimeMs = ElapsedTimeMs;
        // We need to clear these entries from PdaLog and CompletedGoals to make sure that the client, when reconnecting, doesn't have false information
        foreach (string eventKey in auroraEvents)
        {
            pdaStateData.PdaLog.RemoveAll(entry => entry.Key == eventKey);
            storyGoalData.CompletedGoals.Remove(eventKey);
        }
        playerManager.SendPacketToAllPlayers(new AuroraRestore());
        Log.Info($"Restored Aurora, will explode again in {GetMinutesBeforeAuroraExplosion()} minutes");
    }

    /// <summary>
    /// Calculate the future Aurora's explosion time in a deterministic manner
    /// </summary>
    private double GenerateDeterministicAuroraTime(string seed)
    {
        // Copied from CrashedShipExploder.SetExplodeTime() and changed from seconds to ms
        DeterministicGenerator generator = new(seed, nameof(StoryManager));
        return elapsedTimeOutsideStopWatchMs + generator.NextDouble(2.3d, 4d) * 1200d * 1000d;
    }

    /// <summary>
    /// Restarts time counting
    /// </summary>
    public void StartWorld()
    {
        stopWatch.Start();
        playerManager.SendPacketToAllPlayers(MakeTimePacket());
    }

    /// <summary>
    /// Pauses time counting
    /// </summary>
    public void PauseWorld()
    {
        stopWatch.Stop();
    }

    /// <summary>
    /// Calculates the time before the aurora explosion
    /// </summary>
    /// <returns>The time in minutes before aurora explodes or -1 if it already exploded</returns>
    private double GetMinutesBeforeAuroraExplosion()
    {
        return AuroraExplosionTimeMs > ElapsedTimeMs ? Math.Round((AuroraExplosionTimeMs - ElapsedTimeMs) / 60000) : -1;
    }

    /// <summary>
    /// Makes a nice status for the summary command for example
    /// </summary>
    public string GetAuroraStateSummary()
    {
        double minutesBeforeExplosion = GetMinutesBeforeAuroraExplosion();
        if (minutesBeforeExplosion < 0)
        {
            return "already exploded";
        }
        string stateNumber = "";
        if (auroraEvents.Count > 0)
        {
            // Event timer events should always have a number at last character
            string nextEventKey = auroraEvents[0];
            stateNumber = $" [{nextEventKey.Last()}/4]";
        }
        
        return $"explodes in {minutesBeforeExplosion} minutes{stateNumber}";
    }

    internal void ResetWorld()
    {
        stopWatch.Reset();
    }

    /// <summary>
    /// Set current time (replication of SN's system)
    /// </summary>
    /// <param name="type">Type of the operation to apply</param>
    public void ChangeTime(TimeModification type)
    {
        switch (type)
        {
            case TimeModification.DAY:
                ElapsedSeconds += 1200 - ElapsedSeconds % 1200 + 600;
                break;
            case TimeModification.NIGHT:
                ElapsedSeconds += 1200 - ElapsedSeconds % 1200;
                break;
            case TimeModification.SKIP:
                ElapsedSeconds += 600 - ElapsedSeconds % 600;
                break;
        }

        playerManager.SendPacketToAllPlayers(MakeTimePacket());
    }

    public TimeChange MakeTimePacket()
    {
        return new(ElapsedSeconds, DateTimeOffset.Now.ToUnixTimeMilliseconds());
    }

    public InitialTimeData GetInitialTimeData()
    {
        return new(MakeTimePacket(), new((float)AuroraExplosionTimeMs * 0.001f, (float)AuroraWarningTimeMs * 0.001f));
    }

    public enum TimeModification
    {
        DAY, NIGHT, SKIP
    }
}
