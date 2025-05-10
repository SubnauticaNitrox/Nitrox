using NitroxClient.Communication.Abstract;
using NitroxModel;
using Nitrox.Model.Subnautica.DataStructures;
using NitroxModel.Networking.Packets;
using Story;

namespace NitroxClient.Communication.Packets.Processors;

public class StoryGoalExecutedClientIProcessor : IClientPacketProcessor<StoryGoalExecuted>
{
    private readonly IPacketSender packetSender;

    public StoryGoalExecutedClientIProcessor(IPacketSender packetSender)
    {
        this.packetSender = packetSender;
    }

    public Task Process(IPacketProcessContext context, StoryGoalExecuted packet)
    {
        StoryGoalScheduler.main.schedule.RemoveAllFast(packet.Key, static (goal, packetGoalKey) => goal.goalKey == packetGoalKey);

        using (PacketSuppressor<StoryGoalExecuted>.Suppress())
        using (PacketSuppressor<PdaLogEntryAdd>.Suppress())
        using (PacketSuppressor<KnownTechEntryAdd>.Suppress()) // StoryGoalManager => OnGoalUnlockTracker => UnlockBlueprintData => KnownTech.Add
        using (PacketSuppressor<PdaEncyclopediaEntryAdd>.Suppress())
        {
            StoryGoal.Execute(packet.Key, packet.Type.ToUnity());
        }

        return Task.CompletedTask;
    }
}
