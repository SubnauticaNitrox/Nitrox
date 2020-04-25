﻿using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets.Core;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PDAEntryRemove : Packet, IVolatilePacket
    {
        public TechType TechType;
        public float Progress;
        public int Unlocked;

        public PDAEntryRemove(TechType techType, float progress, int unlocked)
        {
            TechType = techType;
            Progress = progress;
            Unlocked = unlocked;
        }

        public override string ToString()
        {
            return "[PDAEntryRemove - techType: " + TechType + " progress: " + Progress + " unlocked: " + Unlocked + "]";
        }
    }
}
