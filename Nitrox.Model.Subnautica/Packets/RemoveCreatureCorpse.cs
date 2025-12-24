using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

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
