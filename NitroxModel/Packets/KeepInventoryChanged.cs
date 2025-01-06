using System;

namespace NitroxModel.Packets;

[Serializable]
public class KeepInventoryChanged : Packet
{
    public bool KeepInventory { get; }
    public KeepInventoryChanged(bool KeepInventory)
    {
        this.KeepInventory = KeepInventory;
    }
}
