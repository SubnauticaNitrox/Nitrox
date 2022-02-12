using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic
{
    public class PDAManagerEntry
    {
        private readonly IPacketSender packetSender;
        public static Dictionary<NitroxTechType, PDAProgressEntry> CachedEntries { get; set; }

        public bool AuroraExplosionTriggered;

        public PDAManagerEntry(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
            AuroraExplosionTriggered = false;
        }

        public void Add(PDAScanner.Entry entry)
        {
            packetSender.Send(new PDAEntryAdd(entry.techType.ToDto(), entry.progress, entry.unlocked));
        }

        public void Progress(PDAScanner.Entry entry, NitroxId nitroxId)
        {
            packetSender.Send(new PDAEntryProgress(entry.techType.ToDto(), entry.progress, entry.unlocked, nitroxId));
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
