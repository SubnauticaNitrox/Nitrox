using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.AppEvents;
using Nitrox.Server.Subnautica.Models.AppEvents.Core;
using Nitrox.Server.Subnautica.Models.GameLogic.Unlockables;
using Nitrox.Server.Subnautica.Models.Helper;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

/// <summary>
///     Keeps track of time and Aurora-related events.
/// </summary>
internal sealed class StoryManager : ISummarize
{
    private readonly ILogger<StoryManager> logger;
    private readonly IOptions<SubnauticaServerOptions> options;
    private readonly PdaManager pdaManager;
    private readonly IPacketSender packetSender;
    private readonly TimeService timeService;

    /// <summary>
    ///     Time at which the Aurora explosion countdown will start (last warning is sent).
    /// </summary>
    /// <remarks>
    ///     It is required to calculate the time at which the Aurora warnings will be sent (along with
    ///     <see cref="AuroraWarningTimeMs" />, look into AuroraWarnings.cs and CrashedShipExploder.cs for more information).
    /// </remarks>
    public double AuroraCountdownTimeMs;

    public TimeSpan AuroraRealExplosionTime;

    /// <summary>
    ///     Time at which the Aurora Events start (you start receiving warnings).
    /// </summary>
    public double AuroraWarningTimeMs;

    public StoryGoalData StoryGoalData { get; set; } = new();

    public StoryManager(IPacketSender packetSender, PdaManager pdaManager, TimeService timeService, IOptions<SubnauticaServerOptions> options, ILogger<StoryManager> logger)
    {
        this.packetSender = packetSender;
        this.pdaManager = pdaManager;
        this.timeService = timeService;
        this.options = options;
        this.logger = logger;

        timeService.TimeSkipped += ReadjustAuroraRealExplosionTime;
    }

    public void ReadjustAuroraRealExplosionTime(TimeSpan skippedTime)
    {
        // Readjust the aurora real explosion time when time skipping because it's based on in-game time
        if (AuroraRealExplosionTime > timeService.ActiveTime)
        {
            TimeSpan newTime = timeService.ActiveTime + skippedTime;
            if (newTime > AuroraRealExplosionTime)
            {
                AuroraRealExplosionTime = timeService.ActiveTime;
            }
            else
            {
                AuroraRealExplosionTime -= skippedTime;
            }
        }
    }

    /// <param name="instantaneous">Whether we should make Aurora explode instantly or after a short countdown</param>
    public void BroadcastExplodeAurora(bool instantaneous)
    {
        // Calculations from CrashedShipExploder.OnConsoleCommand_countdownship()
        // We add 3 seconds to the cooldown (Subnautica adds only 1) so that players have enough time to receive the packet and process it
        AuroraCountdownTimeMs = timeService.GameTime.TotalMilliseconds + 3000;
        AuroraWarningTimeMs = AuroraCountdownTimeMs;
        AuroraRealExplosionTime = timeService.ActiveTime + TimeSpan.FromSeconds(30); // 27 + 3

        if (instantaneous)
        {
            // Calculations from CrashedShipExploder.OnConsoleCommand_explodeship()
            // Removes 25 seconds to the countdown time, jumping to the exact moment of the explosion
            AuroraCountdownTimeMs -= 25000;
            // Is 1 second less than countdown time to have the game understand that we only want the explosion.
            AuroraWarningTimeMs = AuroraCountdownTimeMs - 1000;
            AuroraRealExplosionTime -= TimeSpan.FromSeconds(25);
            logger.ZLogInformation($"Aurora's explosion initiated");
        }
        else
        {
            logger.ZLogInformation($"Aurora's explosion countdown will start in 3 seconds");
        }

        packetSender.SendPacketToAllAsync(new AuroraAndTimeUpdate(GetTimeData(), false));
    }

    public void BroadcastRestoreAurora()
    {
        AuroraWarningTimeMs = timeService.GameTime.TotalMilliseconds;
        AuroraCountdownTimeMs = GenerateDeterministicAuroraTime(options.Value.Seed);
        // Current time + deltaTime before countdown + 27 seconds before explosion
        AuroraRealExplosionTime = timeService.ActiveTime + TimeSpan.FromMilliseconds(AuroraCountdownTimeMs - timeService.GameTime.TotalMilliseconds) + TimeSpan.FromSeconds(27);

        // We need to clear these entries from PdaLog and CompletedGoals to make sure that the client, when reconnecting, doesn't have false information
        foreach (string eventKey in AuroraEventData.GoalNames)
        {
            pdaManager.RemovePdaLogsByKey(eventKey);
            StoryGoalData.CompletedGoals.Remove(eventKey);
        }

        packetSender.SendPacketToAllAsync(new AuroraAndTimeUpdate(GetTimeData(), true));
        logger.ZLogInformation($"Restored Aurora, will explode again in {GetMinutesBeforeAuroraExplosion():@minutes} minutes");
    }

    /// <summary>
    ///     Calculate the time at Aurora's explosion countdown will begin.
    /// </summary>
    /// <remarks>
    ///     Takes the current time into account.
    /// </remarks>
    public double GenerateDeterministicAuroraTime(string seed)
    {
        // Copied from CrashedShipExploder.SetExplodeTime() and changed from seconds to ms
        DeterministicGenerator generator = new(seed, nameof(StoryManager));
        return timeService.GameTime.TotalMilliseconds + generator.NextDouble(2.3d, 4d) * 1200d * 1000d;
    }

    /// <summary>
    ///     Clears the already completed sunbeam events to come and broadcasts it to all players along with the rescheduling of
    ///     the specified sunbeam event.
    /// </summary>
    public void StartSunbeamEvent(string sunbeamEventKey)
    {
        int beginIndex = PlaySunbeamEvent.SunbeamGoals.GetIndex(sunbeamEventKey);
        if (beginIndex == -1)
        {
            logger.ZLogError($"Couldn't find the corresponding sunbeam event in {nameof(PlaySunbeamEvent.SunbeamGoals)} for key {sunbeamEventKey}");
            return;
        }
        for (int i = beginIndex; i < PlaySunbeamEvent.SunbeamGoals.Length; i++)
        {
            StoryGoalData.CompletedGoals.Remove(PlaySunbeamEvent.SunbeamGoals[i]);
        }
        packetSender.SendPacketToAllAsync(new PlaySunbeamEvent(sunbeamEventKey));
    }

    public AuroraEventData MakeAuroraData()
    {
        return new((float)AuroraCountdownTimeMs * 0.001f, (float)AuroraWarningTimeMs * 0.001f, (float)AuroraRealExplosionTime.TotalSeconds);
    }

    public TimeData GetTimeData()
    {
        return new(timeService.MakeTimePacket(), MakeAuroraData());
    }

    public bool RemovedLatestRadioMessage()
    {
        if (StoryGoalData.RadioQueue.Count <= 0)
        {
            return false;
        }

        string message = StoryGoalData.RadioQueue.Dequeue();
        // Just like StoryGoalManager.ExecutePendingRadioMessage
        StoryGoalData.CompletedGoals.Add($"OnPlay{message}");

        return true;
    }

    public InitialStoryGoalData GetInitialStoryGoalData(StoryScheduler storyScheduler, Player player)
    {
        return new InitialStoryGoalData([..StoryGoalData.CompletedGoals], [..StoryGoalData.RadioQueue], storyScheduler.GetScheduledStories(), new(player.PersonalCompletedGoalsWithTimestamp));
    }

    public bool ContainsCompletedStory(string storyGoalKey) => StoryGoalData.CompletedGoals.Contains(storyGoalKey);

    /// <summary>
    ///     Marks a story as completed.
    /// </summary>
    public bool AddCompletedStory(string goalKey) => StoryGoalData.CompletedGoals.Add(goalKey);

    public void QueueRadioStory(string goalKey)
    {
        StoryGoalData.RadioQueue.Enqueue(goalKey);
    }

    Task IEvent<ISummarize.Args>.OnEventAsync(ISummarize.Args args)
    {
        logger.ZLogInformation($"Aurora's state: {GetAuroraStateSummary()}");
        logger.ZLogInformation($"Scheduled goals stored: {StoryGoalData.ScheduledGoals.Count}");
        logger.ZLogInformation($"Story goals completed: {StoryGoalData.CompletedGoals.Count}");
        logger.ZLogInformation($"Unplayed radio messages: {StoryGoalData.RadioQueue.Count}");

        return Task.CompletedTask;

        string GetAuroraStateSummary()
        {
            double minutesBeforeExplosion = GetMinutesBeforeAuroraExplosion();
            if (minutesBeforeExplosion < 0)
            {
                return "already exploded";
            }
            // Based on AuroraWarnings.Update calculations
            // auroraWarningNumber is the amount of received Aurora warnings (there are 4 in total)
            int auroraWarningNumber = 0;
            if (timeService.GameTime.TotalMilliseconds >= AuroraCountdownTimeMs)
            {
                auroraWarningNumber = 4;
            }
            else if (timeService.GameTime.TotalMilliseconds >= Mathf.Lerp((float)AuroraWarningTimeMs, (float)AuroraCountdownTimeMs, 0.8f))
            {
                auroraWarningNumber = 3;
            }
            else if (timeService.GameTime.TotalMilliseconds >= Mathf.Lerp((float)AuroraWarningTimeMs, (float)AuroraCountdownTimeMs, 0.5f))
            {
                auroraWarningNumber = 2;
            }
            else if (timeService.GameTime.TotalMilliseconds >= Mathf.Lerp((float)AuroraWarningTimeMs, (float)AuroraCountdownTimeMs, 0.2f))
            {
                auroraWarningNumber = 1;
            }

            return $"explodes in {minutesBeforeExplosion} minutes [{auroraWarningNumber}/4]";
        }
    }

    /// <returns>Either the time in before Aurora explodes or -1 if it has already exploded.</returns>
    private double GetMinutesBeforeAuroraExplosion()
    {
        return AuroraCountdownTimeMs > timeService.GameTime.TotalMilliseconds ? Math.Round((AuroraCountdownTimeMs - timeService.GameTime.TotalMilliseconds) / 60000) : -1;
    }

    public enum TimeModification
    {
        DAY, NIGHT, SKIP
    }
}
