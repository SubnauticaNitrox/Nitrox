using System;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Helper;
using NitroxServer.GameLogic.Unlockables;
using NitroxModel.Helper;
using NitroxModel;

namespace NitroxServer.GameLogic;

/// <summary>
/// Keeps track of time and Aurora-related events.
/// </summary>
public class StoryManager
{
    private readonly PlayerManager playerManager;
    private readonly PDAStateData pdaStateData;
    private readonly StoryGoalData storyGoalData;
    private readonly TimeKeeper timeKeeper;
    private readonly string seed;

    /// <summary>
    /// Time at which the Aurora explosion countdown will start (last warning is sent).
    /// </summary>
    /// <remarks>
    /// It is required to calculate the time at which the Aurora warnings will be sent (along with <see cref="AuroraWarningTimeMs"/>, look into AuroraWarnings.cs and CrashedShipExploder.cs for more information).
    /// </remarks>
    public double AuroraCountdownTimeMs;
    /// <summary>
    /// Time at which the Aurora Events start (you start receiving warnings).
    /// </summary>
    public double AuroraWarningTimeMs;

    private double ElapsedMilliseconds => timeKeeper.ElapsedMilliseconds;
    private double ElapsedSeconds => timeKeeper.ElapsedSeconds;

    public StoryManager(PlayerManager playerManager, PDAStateData pdaStateData, StoryGoalData storyGoalData, TimeKeeper timeKeeper, string seed, double? auroraExplosionTime, double? auroraWarningTime)
    {
        this.playerManager = playerManager;
        this.pdaStateData = pdaStateData;
        this.storyGoalData = storyGoalData;
        this.timeKeeper = timeKeeper;
        this.seed = seed;
        
        AuroraCountdownTimeMs = auroraExplosionTime ?? GenerateDeterministicAuroraTime(seed);
        AuroraWarningTimeMs = auroraWarningTime ?? ElapsedMilliseconds;
    }

    /// <param name="instantaneous">Whether we should make Aurora explode instantly or after a short countdown</param>
    public void BroadcastExplodeAurora(bool instantaneous)
    {
        // Calculations from CrashedShipExploder.OnConsoleCommand_countdownship()
        // We add 3 seconds to the cooldown (Subnautica adds only 1) so that players have enough time to receive the packet and process it
        AuroraCountdownTimeMs = ElapsedMilliseconds + 3000;
        AuroraWarningTimeMs = AuroraCountdownTimeMs;

        if (instantaneous)
        {
            // Calculations from CrashedShipExploder.OnConsoleCommand_explodeship()
            // Removes 25 seconds to the countdown time, jumping to the exact moment of the explosion
            AuroraCountdownTimeMs -= 25000;
            // Is 1 second less than countdown time to have the game understand that we only want the explosion.
            AuroraWarningTimeMs = AuroraCountdownTimeMs - 1000;
            Log.Info("Aurora's explosion initiated");
        }
        else
        {
            Log.Info("Aurora's explosion countdown will start in 3 seconds");
        }

        playerManager.SendPacketToAllPlayers(new AuroraAndTimeUpdate(GetTimeData(), false));
    }

    public void BroadcastRestoreAurora()
    {
        AuroraWarningTimeMs = ElapsedMilliseconds;
        AuroraCountdownTimeMs = GenerateDeterministicAuroraTime(seed);

        // We need to clear these entries from PdaLog and CompletedGoals to make sure that the client, when reconnecting, doesn't have false information
        foreach (string eventKey in AuroraEventData.GoalNames)
        {
            pdaStateData.PdaLog.RemoveAll(entry => entry.Key == eventKey);
            storyGoalData.CompletedGoals.Remove(eventKey);
        }

        playerManager.SendPacketToAllPlayers(new AuroraAndTimeUpdate(GetTimeData(), true));
        Log.Info($"Restored Aurora, will explode again in {GetMinutesBeforeAuroraExplosion()} minutes");
    }

    /// <summary>
    /// Calculate the time at Aurora's explosion countdown will begin.
    /// </summary>
    /// <remarks>
    /// Takes the current time into account.
    /// </remarks>
    private double GenerateDeterministicAuroraTime(string seed)
    {
        // Copied from CrashedShipExploder.SetExplodeTime() and changed from seconds to ms
        DeterministicGenerator generator = new(seed, nameof(StoryManager));
        return ElapsedMilliseconds + generator.NextDouble(2.3d, 4d) * 1200d * 1000d;
    }

    /// <summary>
    /// Clears the already completed sunbeam events to come and broadcasts it to all players along with the rescheduling of the specified sunbeam event.
    /// </summary>
    public void StartSunbeamEvent(string sunbeamEventKey)
    {
        int beginIndex = PlaySunbeamEvent.SunbeamGoals.GetIndex(sunbeamEventKey);
        if (beginIndex == -1)
        {
            Log.Error($"Couldn't find the corresponding sunbeam event in {nameof(PlaySunbeamEvent.SunbeamGoals)} for key {sunbeamEventKey}");
            return;
        }
        for (int i = beginIndex; i < PlaySunbeamEvent.SunbeamGoals.Length; i++)
        {
            storyGoalData.CompletedGoals.Remove(PlaySunbeamEvent.SunbeamGoals[i]);
        }
        playerManager.SendPacketToAllPlayers(new PlaySunbeamEvent(sunbeamEventKey));
    }

    /// <returns>Either the time in before Aurora explodes or -1 if it has already exploded.</returns>
    private double GetMinutesBeforeAuroraExplosion()
    {
        return AuroraCountdownTimeMs > ElapsedMilliseconds ? Math.Round((AuroraCountdownTimeMs - ElapsedMilliseconds) / 60000) : -1;
    }

    /// <summary>
    /// Makes a nice status of the Aurora events progress for the summary command.
    /// </summary>
    public string GetAuroraStateSummary()
    {
        double minutesBeforeExplosion = GetMinutesBeforeAuroraExplosion();
        if (minutesBeforeExplosion < 0)
        {
            return "already exploded";
        }
        // Based on AuroraWarnings.Update calculations
        // auroraWarningNumber is the amount of received Aurora warnings (there are 4 in total)
        int auroraWarningNumber = 0;
        if (ElapsedMilliseconds >= AuroraCountdownTimeMs)
        {
            auroraWarningNumber = 4;
        }
        else if (ElapsedMilliseconds >= Mathf.Lerp((float)AuroraWarningTimeMs, (float)AuroraCountdownTimeMs, 0.8f))
        {
            auroraWarningNumber = 3;
        }
        else if (ElapsedMilliseconds >= Mathf.Lerp((float)AuroraWarningTimeMs, (float)AuroraCountdownTimeMs, 0.5f))
        {
            auroraWarningNumber = 2;
        }
        else if (ElapsedMilliseconds >= Mathf.Lerp((float)AuroraWarningTimeMs, (float)AuroraCountdownTimeMs, 0.2f))
        {
            auroraWarningNumber = 1;
        }
        
        return $"explodes in {minutesBeforeExplosion} minutes [{auroraWarningNumber}/4]";
    }

    public AuroraEventData MakeAuroraData()
    {
        return new((float)AuroraCountdownTimeMs * 0.001f, (float)AuroraWarningTimeMs * 0.001f);
    }

    public TimeData GetTimeData()
    {
        return new(timeKeeper.MakeTimePacket(), MakeAuroraData());
    }

    public enum TimeModification
    {
        DAY, NIGHT, SKIP
    }
}
