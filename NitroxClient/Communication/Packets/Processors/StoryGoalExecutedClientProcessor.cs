using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using Story;

namespace NitroxClient.Communication.Packets.Processors;

public class StoryGoalExecutedClientProcessor : ClientPacketProcessor<StoryGoalExecuted>
{
    private readonly IPacketSender packetSender;

    public StoryGoalExecutedClientProcessor(IPacketSender packetSender)
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
            StoryGoal.Execute(packet.Key, packet.Type.ToUnity());
        }
    }
}
