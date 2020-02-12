using System;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PDAScannerEntryProgressProcessor : ClientPacketProcessor<PDAEntryProgress>
    {
        private readonly IPacketSender packetSender;

        public PDAScannerEntryProgressProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(PDAEntryProgress packet)
        {
            using (packetSender.Suppress<PDAEntryAdd>())
            using (packetSender.Suppress<PDAEntryProgress>())
            {
                TechType techType = packet.TechType.Enum();
                PDAScanner.EntryData entryData = PDAScanner.GetEntryData(techType);

                PDAScanner.Entry entry;
                if (PDAScanner.GetPartialEntryByKey(techType, out entry))
                {
                    if (packet.Unlocked > entry.unlocked)
                    {
                        Log.Instance.LogMessage(LogCategory.Info, "PDAEntryProgress Upldate Old:" + entry.unlocked + " New" + packet.Unlocked);
                        entry.unlocked = packet.Unlocked;
                    }
                }
                else
                {
                    Log.Instance.LogMessage(LogCategory.Info, "PDAEntryProgress New TechType:" + techType + " Unlocked:" + packet.Unlocked);
                    MethodInfo methodAdd = typeof(PDAScanner).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(TechType), typeof(int) }, null);
                    entry = (PDAScanner.Entry)methodAdd.Invoke(null, new object[] { techType, packet.Unlocked });
                }
            }
        }
    }
}
