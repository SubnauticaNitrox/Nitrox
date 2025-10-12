using System;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PermsChanged : Packet
{
    public Perms NewPerms;

    public PermsChanged(Perms newPerms)
    {
        NewPerms = newPerms;
    }
}
