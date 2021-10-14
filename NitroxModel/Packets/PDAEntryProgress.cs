using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PDAEntryProgress : Packet
    {
        public NitroxTechType TechType { get; }
        public float Progress { get; }
        public int Unlocked { get; }
        public string NitroxId { get; }

        public PDAEntryProgress(NitroxTechType techType, float progress, int unlocked, string nitroxId)
        {
            TechType = techType;
            Progress = progress;
            Unlocked = unlocked;
            NitroxId = nitroxId;
        }
    }
}
