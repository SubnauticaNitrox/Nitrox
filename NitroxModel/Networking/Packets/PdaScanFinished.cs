using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record PdaScanFinished : Packet
{
    public NitroxId Id { get; }
    public NitroxTechType TechType { get; }
    public int UnlockedAmount { get; }
    public bool FullyResearched { get; }
    public bool Destroy { get; }
    public bool WasAlreadyResearched { get; }

    public PdaScanFinished(NitroxId id, NitroxTechType techType,  int unlockedAmount, bool fullyResearched, bool destroy, bool wasAlreadyResearched = false)
    {
        Id = id;
        TechType = techType;
        UnlockedAmount = unlockedAmount;
        FullyResearched = fullyResearched;
        Destroy = destroy;
        WasAlreadyResearched = wasAlreadyResearched;
    }
}
