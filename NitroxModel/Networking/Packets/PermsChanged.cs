using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record PermsChanged : Packet
{
    public Perms NewPerms;

    public PermsChanged(Perms newPerms)
    {
        NewPerms = newPerms;
    }
}
