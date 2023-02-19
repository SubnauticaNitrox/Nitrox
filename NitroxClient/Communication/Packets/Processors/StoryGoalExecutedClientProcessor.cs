using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel;
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
        StoryGoalScheduler.main.schedule.RemoveAllFast(packet.Key, static (goal, packetGoalKey) => goal.goalKey == packetGoalKey);

        using (packetSender.Suppress<StoryGoalExecuted>())
        using (packetSender.Suppress<PDALogEntryAdd>())
        using (packetSender.Suppress<KnownTechEntryAdd>()) // StoryGoalManager => OnGoalUnlockTracker => UnlockBlueprintData => KnownTech.Add
        using (packetSender.Suppress<PDAEncyclopediaEntryAdd>())
        {
            StoryGoal.Execute(packet.Key, packet.Type.ToUnity());
        }
    }
}
