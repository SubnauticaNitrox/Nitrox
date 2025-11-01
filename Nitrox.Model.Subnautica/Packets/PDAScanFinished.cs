using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PDAScanFinished : Packet
{
    public NitroxId Id { get; }
    public NitroxTechType TechType { get; }
    public int UnlockedAmount { get; }
    public bool FullyResearched { get; }
    public bool Destroy { get; }
    public bool WasAlreadyResearched { get; }

    public PDAScanFinished(NitroxId id, NitroxTechType techType,  int unlockedAmount, bool fullyResearched, bool destroy, bool wasAlreadyResearched = false)
    {
        Id = id;
        TechType = techType;
        UnlockedAmount = unlockedAmount;
        FullyResearched = fullyResearched;
        Destroy = destroy;
        WasAlreadyResearched = wasAlreadyResearched;
    }
}
