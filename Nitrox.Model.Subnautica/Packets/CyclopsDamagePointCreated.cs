using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class CyclopsDamagePointCreated : Packet
{
    public NitroxId Id { get; }
    public int DamagePointIndex { get; }

    /// <param name="id">The Cyclops id</param>
    /// <param name="damagePointIndex">
    ///     The created point's index in <see cref="CyclopsExternalDamageManager.damagePoints" />
    /// </param>
    public CyclopsDamagePointCreated(NitroxId id, int damagePointIndex)
    {
        Id = id;
        DamagePointIndex = damagePointIndex;
    }
}
