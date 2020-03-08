using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using Story;

namespace NitroxClient.GameLogic.InitialSync
{
    public class StoryGoalInitialSyncProcessor : InitialSyncProcessor
    {
        
        public override void Process(InitialPlayerSync packet)
        {
            SetCompletedStoryGoals(packet.StoryGoalData.CompletedGoals);
            SetRadioQueue(packet.StoryGoalData.RadioQueue);
            SetGoalUnlocks(packet.StoryGoalData.GoalUnlocks);
        }

        private void SetRadioQueue(List<string> radioQueue)
        {
            StoryGoalManager.main.pendingRadioMessages.AddRange(radioQueue);
            StoryGoalManager.main.PulsePendingMessages();
        }
        
        private void SetCompletedStoryGoals(List<string> storyGoalData)
        {
            StoryGoalManager.main.completedGoals.Clear();
            foreach (string completedGoal in storyGoalData)
            {
                StoryGoalManager.main.completedGoals.Add(completedGoal);
            }
            Log.Info("Received initial sync packet with " + storyGoalData.Count + " completed story goals");
        }
        
        private void SetGoalUnlocks(List<string> goalUnlocks)
        {
            foreach (string goalUnlock in goalUnlocks)
            {
                StoryGoalManager.main.onGoalUnlockTracker.NotifyGoalComplete(goalUnlock);
            }
        }
    }
}
