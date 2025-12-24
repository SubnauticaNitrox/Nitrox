using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class EntityDestroyed : Packet
{
    public NitroxId Id { get; }

    public NitroxTechType TechType { get; }

    public NitroxVector3? LastKnownPosition { get; }

    public EntityDestroyed(NitroxId id, NitroxTechType techType = null, NitroxVector3? lastKnownPosition = null)
    {
        Id = id;
        TechType = techType;
        LastKnownPosition = lastKnownPosition;
    }
}
