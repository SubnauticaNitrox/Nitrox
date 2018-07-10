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
        public PDAStateData PDAState { get; set; }

    }
}
