using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Packets;

[Serializable]
public class RemoveCreatureCorpse : Packet
{
    public NitroxId CreatureId { get; }
    public NitroxVector3 DeathPosition { get; }
    public NitroxQuaternion DeathRotation { get; }

    public RemoveCreatureCorpse(NitroxId creatureId, NitroxVector3 deathPosition, NitroxQuaternion deathRotation)
    {
        CreatureId = creatureId;
        DeathPosition = deathPosition;
        DeathRotation = deathRotation;
    }
}
