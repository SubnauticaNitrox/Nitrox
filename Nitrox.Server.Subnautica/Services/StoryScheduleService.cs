using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Keeps track of PDA story goals and scheduled story events.
/// </summary>
internal class StoryScheduleService(TimeService timeService, IServerPacketSender packetSender)
    : IHostedService
{
    private readonly ThreadSafeDictionary<string, NitroxScheduledGoal> scheduledGoals = new();
    private readonly TimeService timeService = timeService;
    private readonly IServerPacketSender packetSender = packetSender;

    public List<NitroxScheduledGoal> GetScheduledGoals() => scheduledGoals.Values.ToList();

    public bool ContainsScheduledGoal(string goalKey) => scheduledGoals.ContainsKey(goalKey);

    public void ScheduleGoal(NitroxScheduledGoal scheduledGoal)
    {
        // Only add if it's not in already
        if (!scheduledGoals.ContainsKey(scheduledGoal.GoalKey))
        {
            // If it's not already in any PDA stuff (completed goals or PDALog)
            if (!IsAlreadyRegistered(scheduledGoal.GoalKey))
            {
                if (scheduledGoal.TimeExecute > (float)timeService.Elapsed.TotalSeconds)
                {
                    scheduledGoals.Add(scheduledGoal.GoalKey, scheduledGoal);
                }
            }
        }
    }

    /// <param name="goalKey">The goal key/name.</param>
    /// <param name="becauseOfTime">
    ///     When the server starts, it happens that there are still some goals that were supposed to happen
    ///     but didn't, so to make sure that they happen on at least one client, we postpone its execution
    /// </param>
    public void UnScheduleGoal(string goalKey, bool becauseOfTime = false)
    {
        if (!scheduledGoals.TryGetValue(goalKey, out NitroxScheduledGoal scheduledGoal))
        {
            return;
        }
        // The best solution, to ensure any bad simulation of client side, is to postpone the execution
        // If the goal is already done, no need to check anything
        if (becauseOfTime && !IsAlreadyRegistered(goalKey))
        {
            scheduledGoal.TimeExecute = (float)timeService.Elapsed.TotalSeconds + 15;
            packetSender.SendPacketToAll(new Schedule(scheduledGoal.TimeExecute, goalKey, scheduledGoal.GoalCategory));
            return;
        }
        scheduledGoals.Remove(goalKey);
    }

    public bool IsAlreadyRegistered(string goalKey)
    {
        // TODO: USE DATBASE
        // return pda.State.PdaLog.Any(entry => entry.Key == goalKey) || storyGoal.State.CompletedGoals.Contains(goalKey);
        return false; // TODO REMOVE
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // TODO: USE DATABASE
        // StoryGoalData story = await storyGoal.GetStateAsync(cancellationToken);
        // // We still want to get a "replicated" list in memory
        // for (int i = story.ScheduledGoals.Count - 1; i >= 0; i--)
        // {
        //     NitroxScheduledGoal scheduledGoal = story.ScheduledGoals[i];
        //     // In the unlikely case that there's a duplicated entry
        //     if (scheduledGoals.TryGetValue(scheduledGoal.GoalKey, out NitroxScheduledGoal alreadyInGoal))
        //     {
        //         // We remove the goal that's already in if it's planned for later than the first one
        //         if (scheduledGoal.TimeExecute <= alreadyInGoal.TimeExecute)
        //         {
        //             UnScheduleGoal(alreadyInGoal.GoalKey);
        //         }
        //         continue;
        //     }
        //
        //     scheduledGoals.Add(scheduledGoal.GoalKey, scheduledGoal);
        // }
        //
        // await pda.GetStateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
