using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Core;
using Story;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class StoryGoalExecutedClientProcessor(IPacketSender packetSender) : IClientPacketProcessor<StoryGoalExecuted>
{
    private readonly IPacketSender packetSender = packetSender;

    public Task Process(ClientProcessorContext context, StoryGoalExecuted packet)
    {
        StoryGoalScheduler.main.schedule.RemoveAllFast(packet.Key, static (goal, packetGoalKey) => goal.goalKey == packetGoalKey);

        using (PacketSuppressor<StoryGoalExecuted>.Suppress())
        using (PacketSuppressor<PDALogEntryAdd>.Suppress())
        using (PacketSuppressor<KnownTechEntryAdd>.Suppress()) // StoryGoalManager => OnGoalUnlockTracker => UnlockBlueprintData => KnownTech.Add
        using (PacketSuppressor<PDAEncyclopediaEntryAdd>.Suppress())
        {
            StoryGoal.Execute(packet.Key, packet.Type.ToUnity());
        }
        return Task.CompletedTask;
    }
}
