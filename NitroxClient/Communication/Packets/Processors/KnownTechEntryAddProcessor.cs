using System;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Core;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class KnownTechEntryProcessorAdd : IClientPacketProcessor<KnownTechEntryAdd>
{
    public Task Process(ClientProcessorContext context, KnownTechEntryAdd packet)
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
