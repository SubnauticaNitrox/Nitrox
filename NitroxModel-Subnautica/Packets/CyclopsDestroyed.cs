using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets;

[Serializable]
public class CyclopsDestroyed : Packet
{
    public NitroxId Id { get; }
    /// <summary>
    /// Whether the cyclops was destroyed by a command or not
    /// </summary>
    public bool Instantly { get; }

    public CyclopsDestroyed(NitroxId id, bool instantly)
    {
        Id = id;
        Instantly = instantly;
    }

    public override string ToString()
    {
        return $"[CyclopsDestroyed - Id: {Id}, Instantly: {Instantly}]";
    }
}
