using System;

namespace NitroxModel.Packets;

[Serializable]
public class KeepInventoryChanged : Packet
{
    public bool KeepInventoryOnDeath { get; }

    public KeepInventoryChanged(bool keepInventoryOnDeath)
    {
        KeepInventoryOnDeath = keepInventoryOnDeath;
    }
}
