using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using ProtoBufNet;

namespace NitroxServer.GameLogic.Unlockables
{
    [ProtoContract]
    public class StoryGoalData
    {
        [ProtoMember(1)]
        public List<string> SerializeCompletedGoals
        {
            get
            {
                lock (completedGoals)
                {
                    return completedGoals;
                }
            }
            set { completedGoals = value; }
        }

        [ProtoMember(2)]
        public List<string> SerializeRadioQueue
        {
            get
            {
                lock (radioQueue)
                {
                    return radioQueue;
                }
            }
            set { radioQueue = value; }
        }

        [ProtoIgnore]
        private List<string> completedGoals = new List<string>();
        
        [ProtoIgnore]
        private List<string> radioQueue = new List<string>();

        public void AddStoryGoal(string entry)
        {
            lock (completedGoals)
            {
                completedGoals.Add(entry);
            }
        }

        public void AddRadioMessage(string entry)
        {
            lock (radioQueue)
            {
                radioQueue.Add(entry);
            }
        }

        public void RemovedLatestRadioMessage()
        {
            lock (radioQueue)
            {
                radioQueue.RemoveAt(0);
            }
        }

        public InitialStoryGoalData GetInitialStoryGoalData()
        {
            lock (completedGoals)
            {
                lock (radioQueue)
                {
                    return new InitialStoryGoalData(new List<string>(completedGoals), new List<string>(radioQueue));
                }
            }
        }
    }
}
