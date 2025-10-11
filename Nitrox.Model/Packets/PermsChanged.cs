using System;
using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Model.Packets;

[Serializable]
public class PermsChanged : Packet
{
    public Perms NewPerms;

    public PermsChanged(Perms newPerms)
    {
        NewPerms = newPerms;
    }
}
