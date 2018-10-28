using System;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PDAScannerEntryProgressProcessor : ClientPacketProcessor<PDAEntryProgress>
    {
        private readonly IPacketSender packetSender;
        private readonly INitroxLogger log;

        public PDAScannerEntryProgressProcessor(IPacketSender packetSender, INitroxLogger logger)
        {
            this.packetSender = packetSender;
            log = logger;
        }

        public override void Process(PDAEntryProgress packet)
        {
            using (packetSender.Suppress<PDAEntryAdd>())
            using (packetSender.Suppress<PDAEntryProgress>())
            {
                PDAScanner.EntryData entryData = PDAScanner.GetEntryData(packet.TechType);

                PDAScanner.Entry entry;
                if (PDAScanner.GetPartialEntryByKey(packet.TechType, out entry))
                {
                    if (packet.Unlocked > entry.unlocked)
                    {
                        log.Info($"PDAEntryProgress Update Old: {entry.unlocked} New {packet.Unlocked}");
                        entry.unlocked = packet.Unlocked;
                    }
                }
                else
                {
                    log.Info($"PDAEntryProgress New TechType: {packet.TechType} Unlocked: {packet.Unlocked}");
                    MethodInfo methodAdd = typeof(PDAScanner).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(TechType), typeof(int) }, null);
                    entry = (PDAScanner.Entry)methodAdd.Invoke(null, new object[] { packet.TechType, packet.Unlocked });
                }
            }
        }
    }
}
