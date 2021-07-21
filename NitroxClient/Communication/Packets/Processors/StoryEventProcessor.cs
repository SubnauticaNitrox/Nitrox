using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Packets;
using Story;

namespace NitroxClient.Communication.Packets.Processors
{
    public class StoryEventProcessor : ClientPacketProcessor<StoryEventSend>
    {
        public override void Process(StoryEventSend packet)
        {
            switch (packet.StoryEventType)
            {
                case StoryEventType.PDA:
                case StoryEventType.RADIO:
                case StoryEventType.ENCYCLOPEDIA:
                case StoryEventType.STORY:
                    using (NitroxServiceLocator.LocateService<IPacketSender>().Suppress<StoryEventSend>())
                    {
                        // To avoid hitting the dynamic patch related to story
                        Execute(packet.Key, (Story.GoalType)packet.StoryEventType);
                    }
                    break;
                case StoryEventType.EXTRA:
                    ExecuteExtraEvent(packet.Key);
                    break;
            }
        }

        //Copied from StoryGoal.Execute() 
        private void Execute(string key, Story.GoalType goalType)
        {
            switch (goalType)
            {
                case Story.GoalType.PDA:
                    PDALog.Add(key, true);
                    return;
                case Story.GoalType.Radio:
                    if (StoryGoalManager.main.OnGoalComplete(key))
                    {
                        StoryGoalManager.main.AddPendingRadioMessage(key);
                        return;
                    }
                    break;
                case Story.GoalType.Encyclopedia:
                    PDAEncyclopedia.AddAndPlaySound(key);
                    break;
                default:
                    return;
            }
        }

        private void ExecuteExtraEvent(string key)
        {
            switch (key)
            {
                case "Story_AuroraExplosion":
                    CrashedShipExploder main = CrashedShipExploder.main;
                    main.timeToStartCountdown = ((Utils.ScalarMonitor)main.ReflectionGet("timeMonitor", false, false)).Get() - 25f + 1f;
                    main.timeToStartWarning = main.timeToStartCountdown - 1f;
                    break;
            }
        }
    }
}
