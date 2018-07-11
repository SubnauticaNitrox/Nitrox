using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PDAEntryAdd : Packet
    {
        public TechType TechType;
        public float Progress;
        public int Unlocked;

        public PDAEntryAdd(TechType techType, float progress, int unlocked)
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

    [Serializable]
    public class PDAEntryProgress : Packet
    {
        public TechType TechType;
        public float Progress;
        public int Unlocked;

        public PDAEntryProgress(TechType techType, float progress, int unlocked)
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

    [Serializable]
    public class PDAEntryRemove : Packet
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
