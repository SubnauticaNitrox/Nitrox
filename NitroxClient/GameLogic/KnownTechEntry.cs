using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class KnownTechEntry
    {
        private readonly IPacketSender packetSender;

        public KnownTechEntry(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void Add(TechType techType, bool verbose)
        {
            KnownTechEntryAdd EntryAdd = new KnownTechEntryAdd(techType, verbose);
            packetSender.Send(EntryAdd);
        }
<<<<<<< HEAD
<<<<<<< HEAD
=======

        public void Remove(TechType techType)
        {
            KnownTechEntryRemove EntryChanged = new KnownTechEntryRemove(techType);
            packetSender.Send(EntryChanged);
        }

        public void Progress()
        {
            KnownTechEntryChanged EntryChanged = new KnownTechEntryChanged();
            packetSender.Send(EntryChanged);
        }
>>>>>>> 08eed5b... Sync And Save (KnownTech Entries,PDAScanner Entries,PDAEncyclopediaEntries )
=======
>>>>>>> c7606c2... Changes Requested
    }
}
