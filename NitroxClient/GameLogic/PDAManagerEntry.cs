using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic
{
    public class PDAManagerEntry
    {
        private readonly IPacketSender packetSender;

        public PDAManagerEntry(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void Add(PDAScanner.Entry entry)
        {
            packetSender.Send(new PDAEntryAdd(entry.techType.ToDto(), entry.progress, entry.unlocked));
        }

        public void Progress(PDAScanner.Entry entry)
        {
            packetSender.Send(new PDAEntryProgress(entry.techType.ToDto(), entry.progress, entry.unlocked));
        }

        public void Remove(PDAScanner.Entry entry)
        {
            packetSender.Send(new PDAEntryRemove(entry.techType.ToDto()));
        }

        public void LogAdd(PDALog.Entry entry)
        {
            packetSender.Send(new PDALogEntryAdd(entry.data.key, entry.timestamp));
        }
    }
}
