using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.GameLogic
{
    internal class StoryScheduler(IPacketSender packetSender, PdaManager pdaManager, StoryManager storyManager, TimeService timeService, PlayerManager playerManager)
    {
        private readonly IPacketSender packetSender = packetSender;
        private readonly PdaManager pdaManager = pdaManager;
        private readonly PlayerManager playerManager = playerManager;
        private readonly ThreadSafeDictionary<string, NitroxScheduledGoal> scheduledStories = [];
        private readonly StoryManager storyManager = storyManager;
        private readonly TimeService timeService = timeService;

        private float ElapsedSecondsFloat => (float)timeService.GameTime.TotalSeconds;

        public void ScheduleStoriesIfNotInPast(IList<NitroxScheduledGoal> storyGoals)
        {
            for (int i = storyGoals.Count - 1; i >= 0; i--)
            {
                NitroxScheduledGoal newStoryGoal = storyGoals[i];

                // Ignore story and remove existing story from schedule if it should already be done.
                if (scheduledStories.TryGetValue(newStoryGoal.GoalKey, out NitroxScheduledGoal alreadyScheduledGoal))
                {
                    if (newStoryGoal.TimeExecute <= alreadyScheduledGoal.TimeExecute)
                    {
                        UnscheduleStory(alreadyScheduledGoal.GoalKey);
                    }
                    continue;
                }

                scheduledStories.Add(newStoryGoal.GoalKey, newStoryGoal);
            }
        }

        public List<NitroxScheduledGoal> GetScheduledStories() => scheduledStories.Values.ToList();

        public bool ContainsScheduledStory(string storyGoalKey) => scheduledStories.ContainsKey(storyGoalKey);

        public void ScheduleStory(NitroxScheduledGoal scheduledGoal)
        {
            // Only add if it's not in already
            if (!scheduledStories.ContainsKey(scheduledGoal.GoalKey))
            {
                // If it's not already in any PDA stuff (completed goals or PDALog)
                if (!IsTrackedStory(scheduledGoal.GoalKey))
                {
                    if (scheduledGoal.TimeExecute > ElapsedSecondsFloat)
                    {
                        scheduledStories.Add(scheduledGoal.GoalKey, scheduledGoal);
                    }
                }
            }
        }

        /// <param name="storyGoalKey"></param>
        /// <param name="becauseOfTime">
        ///     When the server starts, it happens that there are still some goals that were supposed to happen
        ///     but didn't, so to make sure that they happen on at least one client, we postpone its execution
        /// </param>
        public void UnscheduleStory(string storyGoalKey, bool becauseOfTime = false)
        {
            if (!scheduledStories.TryGetValue(storyGoalKey, out NitroxScheduledGoal scheduledGoal))
            {
                return;
            }
            // The best solution, to ensure any bad simulation of client side, is to postpone the execution
            // If the goal is already done, no need to check anything
            if (becauseOfTime && !IsTrackedStory(storyGoalKey))
            {
                scheduledGoal.TimeExecute = ElapsedSecondsFloat + 15;
                packetSender.SendPacketToAllAsync(new Schedule(scheduledGoal.TimeExecute, storyGoalKey, scheduledGoal.GoalType));
                return;
            }
            scheduledStories.Remove(storyGoalKey);
        }

        private bool IsTrackedStory(string storyGoalKey) => pdaManager.ContainsLog(storyGoalKey) || storyManager.ContainsCompletedStory(storyGoalKey);
    }
}
