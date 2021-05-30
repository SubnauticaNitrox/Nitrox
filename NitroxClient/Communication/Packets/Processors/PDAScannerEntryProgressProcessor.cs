using System;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PDAScannerEntryProgressProcessor : ClientPacketProcessor<PDAEntryProgress>
    {
        private readonly MethodInfo methodAdd = typeof(PDAScanner).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(TechType), typeof(int) }, null);
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
                TechType techType = packet.TechType.ToUnity();

                if (PDAScanner.GetPartialEntryByKey(techType, out PDAScanner.Entry entry))
                {
                    if (packet.Unlocked > entry.unlocked)
                    {
                        Log.Info($"PDAEntryProgress Update For TechType:{techType} Old:{entry.unlocked} New:{packet.Unlocked}");
                        entry.unlocked = packet.Unlocked;
                    }
                }
                else
                {
                    Log.Info($"PDAEntryProgress New TechType:{techType} Unlocked:{packet.Unlocked}");
                    methodAdd.Invoke(null, new object[] { techType, packet.Unlocked });
                }
            }
        }
    }
}
