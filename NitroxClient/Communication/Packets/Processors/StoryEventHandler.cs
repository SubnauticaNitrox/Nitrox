using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using Story;

namespace NitroxClient.Communication.Packets.Processors
{
    public class StoryEventHandler : ClientPacketProcessor<StoryEventSend>
    {
        private readonly IPacketSender packetSender;
        private readonly PDAManagerEntry pdaManagerEntry;

        public StoryEventHandler(IPacketSender packetSender, PDAManagerEntry pdaManagerEntry)
        {
            this.packetSender = packetSender;
            this.pdaManagerEntry = pdaManagerEntry;
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
                        StoryGoal.Execute(packet.Key, packet.Type.ToUnity());
                    }
                    break;
                case StoryEventSend.EventType.EXTRA:
                    ExecuteExtraEvent(packet.Key);
                    break;
                case StoryEventSend.EventType.PDA_EXTRA:
                    PDALog.entries.Remove(packet.Key);
                    StoryGoal.Execute(packet.Key, Story.GoalType.PDA);
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
            pdaManagerEntry.AuroraExplosionTriggered = true;
            CrashedShipExploder main = CrashedShipExploder.main;
            main.timeMonitor.Update(DayNightCycle.main.timePassedAsFloat);
            main.timeToStartCountdown = main.timeMonitor.Get();
        }
    }
}
