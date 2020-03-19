using System.Collections;
using System.Collections.Generic;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.Logger;
using NitroxModel.Packets;
using Story;

namespace NitroxClient.GameLogic.InitialSync
{
    public class StoryGoalInitialSyncProcessor : InitialSyncProcessor
    {
        
        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            SetCompletedStoryGoals(packet.StoryGoalData.CompletedGoals);
            waitScreenItem.SetProgress(0.33f);
            yield return null;

            SetRadioQueue(packet.StoryGoalData.RadioQueue);
            waitScreenItem.SetProgress(0.66f);
            yield return null;

            SetGoalUnlocks(packet.StoryGoalData.GoalUnlocks);
            waitScreenItem.SetProgress(1f);
            yield return null;
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
