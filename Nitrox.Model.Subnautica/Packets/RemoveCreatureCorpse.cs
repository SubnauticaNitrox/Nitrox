using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class RemoveCreatureCorpse : Packet
{
    public NitroxId CreatureId { get; }

    /// <summary>
    ///     World space position of the creature when it died.
    /// </summary>
    public NitroxVector3 DeathPosition { get; }

    /// <summary>
    ///     World space rotation of the creature when it died.
    /// </summary>
    public NitroxQuaternion DeathRotation { get; }

    public RemoveCreatureCorpse(NitroxId creatureId, NitroxVector3 deathPosition, NitroxQuaternion deathRotation)
    {
        CreatureId = creatureId;
        DeathPosition = deathPosition;
        DeathRotation = deathRotation;
    }
}
