﻿using NitroxServer.GameLogic.Unlockables;
using ProtoBufNet;
using System;

namespace NitroxServer.GameLogic.Bases
{
    [Serializable]
    [ProtoContract]
    public class GameData
    {
        [ProtoMember(1)]
        public PDAStateData PDAState { get; set; }
        
        [ProtoMember(2)]
        public StoryGoalData StoryGoals { get; set; }
    }
}
