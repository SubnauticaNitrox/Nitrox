using System;
using DTO = NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PDAEntryAdd : Packet
    {
        public DTO.TechType TechType;
        public float Progress;
        public int Unlocked;

        public PDAEntryAdd(DTO.TechType techType, float progress, int unlocked)
        {
            TechType = techType;
            Progress = progress;
            Unlocked = unlocked;
        }

        public override string ToString()
        {
            return "[PDAEntryAdd - techType: " + TechType + " progress: " + Progress + " unlocked: " + Unlocked + "]";
        }
    }
}
