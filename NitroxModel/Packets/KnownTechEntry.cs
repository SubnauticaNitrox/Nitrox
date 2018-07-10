using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class KnownTechEntryAdd : Packet
    {
        public TechType TechType;
        public bool Verbose;

        public KnownTechEntryAdd(TechType techType, bool verbose)
        {
            TechType = techType;
            Verbose = verbose;
        }
        public override string ToString()
        {
            return "[KnownTechEntryAdd - TechType: " + TechType + " Verbose: " + Verbose + "]";
        }
    }

<<<<<<< HEAD
<<<<<<< HEAD
    
=======
    public class KnownTechEntryChanged : Packet
    {
        public KnownTechEntryChanged()
        {

        }

        public override string ToString()
        {
            return "[KnownTechEntryChanged]";
        }
    }

    public class KnownTechEntryRemove : Packet
    {
        TechType techType;

        public KnownTechEntryRemove(TechType techType)
        {
            this.techType = techType;
        }

        public override string ToString()
        {
            return "[KnownTechEntryRemove - TechType: " + techType + "]";
        }
    }
>>>>>>> 08eed5b... Sync And Save (KnownTech Entries,PDAScanner Entries,PDAEncyclopediaEntries )
=======
    
>>>>>>> c7606c2... Changes Requested
}
