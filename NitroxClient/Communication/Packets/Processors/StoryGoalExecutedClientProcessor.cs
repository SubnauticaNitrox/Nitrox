using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using Story;

namespace NitroxClient.Communication.Packets.Processors;

public class StoryGoalExecutedClientProcessor : ClientPacketProcessor<StoryGoalExecuted>
{
    private readonly IPacketSender packetSender;

    public StoryGoalExecutedClientProcessor(IPacketSender packetSender, PDAManagerEntry pdaManagerEntry)
    {
        this.packetSender = packetSender;
    }

    public override void Process(StoryGoalExecuted packet)
    {
        StoryGoalScheduler.main.schedule.RemoveAll(goal => goal.goalKey == packet.Key);

        using (packetSender.Suppress<StoryGoalExecuted>())
        using (packetSender.Suppress<PDALogEntryAdd>())
        using (packetSender.Suppress<PDAEncyclopediaEntryAdd>())
        {
            switch (packet.Type)
            {
                case StoryGoalExecuted.EventType.EXTRA:
                    ExecuteExtraEvent(packet.Key);
                    return;
                case StoryGoalExecuted.EventType.PDA_EXTRA:
                    PDALog.entries.Remove(packet.Key);
                    StoryGoal.Execute(packet.Key, Story.GoalType.PDA);
                    return;
            }
            StoryGoal.Execute(packet.Key, packet.Type.ToUnity());
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
        main.timeMonitor.Update(DayNightCycle.main.timePassedAsFloat);
        main.timeToStartCountdown = main.timeMonitor.Get();
    }
}
