using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
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
            switch (packet.Type)
            {
                case StoryEventSend.EventType.PDA:
                case StoryEventSend.EventType.RADIO:
                case StoryEventSend.EventType.ENCYCLOPEDIA:
                case StoryEventSend.EventType.STORY:
                    using (packetSender.Suppress<StoryEventSend>())
                    using (packetSender.Suppress<PDALogEntryAdd>())
                    {
                        StoryGoal.Execute(packet.Key, (Story.GoalType)packet.Type);
                    }
                    break;
                case StoryEventSend.EventType.EXTRA:
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
            main.timeToStartCountdown = main.timeMonitor.Get() - 25f + 1f;
            main.timeToStartWarning = main.timeToStartCountdown - 1f;
        }
    }
}
