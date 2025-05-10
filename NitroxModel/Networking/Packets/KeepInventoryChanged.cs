using System;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record KeepInventoryChanged : Packet
{
    public bool KeepInventoryOnDeath { get; }

    public KeepInventoryChanged(bool keepInventoryOnDeath)
    {
        KeepInventoryOnDeath = keepInventoryOnDeath;
    }
}
