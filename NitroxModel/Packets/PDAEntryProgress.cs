﻿using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PDAEntryProgress : Packet
    {
        public NitroxTechType TechType;
        public float Progress;
        public int Unlocked;

        public PDAEntryProgress(NitroxTechType techType, float progress, int unlocked)
        {
            TechType = techType;
            Progress = progress;
            Unlocked = unlocked;
        }

        public override string ToString()
        {
            return "[PDAEntryProgress - techType: " + TechType + " progress: " + Progress + " unlocked: " + Unlocked + "]";
        }
    }
}
