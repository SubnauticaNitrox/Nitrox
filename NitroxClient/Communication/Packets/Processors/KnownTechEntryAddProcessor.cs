using System;
using NitroxClient.Communication.Abstract;
using Nitrox.Model.Subnautica.DataStructures;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class KnownTechEntryProcessorAdd : IClientPacketProcessor<KnownTechEntryAdd>
{
    private readonly IPacketSender packetSender;

    public KnownTechEntryProcessorAdd(IPacketSender packetSender)
    {
        this.packetSender = packetSender;
    }

    public Task Process(IPacketProcessContext context, KnownTechEntryAdd packet)
    {
        using (PacketSuppressor<KnownTechEntryAdd>.Suppress())
        {
            switch (packet.Category)
            {
                case KnownTechEntryAdd.EntryCategory.KNOWN:
                    KnownTech.Add(packet.TechType.ToUnity(), packet.Verbose);
                    break;
                case KnownTechEntryAdd.EntryCategory.ANALYZED:
                    KnownTech.Analyze(packet.TechType.ToUnity(), packet.Verbose);
                    break;
                default:
                    string categoryName = Enum.GetName(typeof(KnownTechEntryAdd.EntryCategory), packet.Category);
                    Log.Error($"Received an unknown category type for {nameof(KnownTechEntryAdd)} packet: {categoryName}");
                    break;
            }
        }

        return Task.CompletedTask;
    }
}
