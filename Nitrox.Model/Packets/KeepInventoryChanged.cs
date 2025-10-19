using System;

namespace Nitrox.Model.Packets;

[Serializable]
public class KeepInventoryChanged : Packet
{
    public bool KeepInventoryOnDeath { get; }

    public KeepInventoryChanged(bool keepInventoryOnDeath)
    {
        KeepInventoryOnDeath = keepInventoryOnDeath;
    }
}
