using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

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
            PDAEntryAdd EntryChanged = new PDAEntryAdd(entry.techType, entry.progress,entry.unlocked);
            packetSender.Send(EntryChanged);
        }

        public void Progress(PDAScanner.Entry entry)
        {
            PDAEntryProgress EntryChanged = new PDAEntryProgress(entry.techType, entry.progress, entry.unlocked);
            packetSender.Send(EntryChanged);
        }

        public void Remove(PDAScanner.Entry entry)
        {
            PDAEntryRemove EntryChanged = new PDAEntryRemove(entry.techType, entry.progress, entry.unlocked);
            packetSender.Send(EntryChanged);
        }

        public void LogAdd(PDALog.Entry entry)
        {
            PDALogEntryAdd EntryChanged = new PDALogEntryAdd(entry.data.key, entry.timestamp);
            packetSender.Send(EntryChanged);
        }
    }
}
