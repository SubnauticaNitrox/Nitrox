using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets;

[Serializable]
public class PDAScanProgress : Packet
{
    public bool ProgressCompleted => Math.Abs(Progress - 1f) < 0.0005;
    public NitroxId Id { get; }
    public NitroxTechType TechType { get; }
    public float Progress { get; }
    public bool Destroy { get; }

    public PDAScanProgress(NitroxId id, NitroxTechType techType, float progress, bool destroy)
    {
        Id = id;
        TechType = techType;
        Progress = progress;
        Destroy = destroy;
    }
}
