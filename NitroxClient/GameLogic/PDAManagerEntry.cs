using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic
{
    // TODO: Remake this
    public class PDAManagerEntry
    {
        private readonly IPacketSender packetSender;
        public static Dictionary<NitroxTechType, PDAProgressEntry> CachedEntries { get; set; }

        public PDAManagerEntry(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void Add(PDAScanner.Entry entry)
        {
            if (!Multiplayer.Main || !Multiplayer.Main.InitialSyncCompleted)
            {
                return;
            }
            packetSender.Send(new PDAEntryAdd(entry.techType.ToDto(), entry.progress, entry.unlocked));
        }

        public void Progress(PDAScanner.Entry entry, NitroxId nitroxId)
        {
            if (!Multiplayer.Main || !Multiplayer.Main.InitialSyncCompleted)
            {
                return;
            }
            packetSender.Send(new PDAEntryProgress(entry.techType.ToDto(), entry.progress, entry.unlocked, nitroxId));
        }

        public void Remove(PDAScanner.Entry entry)
        {
            if (!Multiplayer.Main || !Multiplayer.Main.InitialSyncCompleted)
            {
                return;
            }
            packetSender.Send(new PDAEntryRemove(entry.techType.ToDto()));
        }

        public void LogAdd(PDALog.Entry entry)
        {
            if (!Multiplayer.Main || !Multiplayer.Main.InitialSyncCompleted)
            {
                return;
            }
            packetSender.Send(new PDALogEntryAdd(entry.data.key, entry.timestamp));
        }
    }
}
