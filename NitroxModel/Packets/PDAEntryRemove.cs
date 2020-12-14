using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PDAEntryRemove : Packet
    {
        public NitroxTechType TechType { get; }
        public float Progress { get; }
        public int Unlocked { get; }

        public PDAEntryRemove(NitroxTechType techType, float progress, int unlocked)
        {
            TechType = techType;
            Progress = progress;
            Unlocked = unlocked;
        }

        public override string ToString()
        {
            return $"[PDAEntryRemove - TechType: {TechType}, Progress: {Progress}, Unlocked: {Unlocked}]";
        }
    }
}
