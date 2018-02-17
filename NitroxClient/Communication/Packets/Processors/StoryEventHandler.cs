using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Packets;
using Story;

namespace NitroxClient.Communication.Packets.Processors
{
    public class StoryEventHandler : ClientPacketProcessor<StoryEventSend>
    {
        public override void Process(StoryEventSend packet)
        {
            switch (packet.StoryEventType)
            {
                case StoryEventType.PDA:
                case StoryEventType.Radio:
                case StoryEventType.Encyclopedia:
                case StoryEventType.Story:
                    using (NitroxServiceLocator.LocateService<IPacketSender>().Suppress<StoryEventSend>())
                    {
                        StoryGoal.Execute(packet.Key, (Story.GoalType)packet.StoryEventType);
                    }
                    break;
                case StoryEventType.Extra:
                    ExecuteExtraEvent(packet.Key);
                    break;
            }
        }

        private void ExecuteExtraEvent(string key)
        {
            switch (key)
            {
                case "Story_AuroraExplosion":
                    ExplodeAurora();
                    break;
            }
        }

        private void ExplodeAurora()
        {
            CrashedShipExploder main = CrashedShipExploder.main;
            main.timeToStartCountdown = ((Utils.ScalarMonitor)main.ReflectionGet("timeMonitor", false, false)).Get() - 25f + 1f;
            main.timeToStartWarning = main.timeToStartCountdown - 1f;
        }
    }
}
