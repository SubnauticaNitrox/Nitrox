using System;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class KeepInventoryChanged : Packet
{
    public bool KeepInventoryOnDeath { get; }

    public KeepInventoryChanged(bool keepInventoryOnDeath)
    {
        KeepInventoryOnDeath = keepInventoryOnDeath;
    }
}
