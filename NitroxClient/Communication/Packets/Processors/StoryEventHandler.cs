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
        private readonly IPacketSender packetSender;

        public StoryEventHandler(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

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
                        StoryGoal.Execute(packet.Key, (Story.GoalType)packet.StoryEventType);
                    }
                    break;
                case StoryEventType.EXTRA:
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
