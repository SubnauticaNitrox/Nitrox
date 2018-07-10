using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;
using System;
using System.Collections.Generic;

namespace NitroxServer.GameLogic.Bases
{
    [Serializable]
    [ProtoContract]
    public class GameData
    {
        [ProtoMember(1)]
<<<<<<< HEAD
<<<<<<< HEAD
        public PDAStateData PDAState { get; set; }
=======
        public PDASaveData PDASaveData { get; set; }
>>>>>>> 08eed5b... Sync And Save (KnownTech Entries,PDAScanner Entries,PDAEncyclopediaEntries )
=======
        public PDAStateData PDAState { get; set; }
>>>>>>> c7606c2... Changes Requested

    }
}
