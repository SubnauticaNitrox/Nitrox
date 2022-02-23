using System;

namespace NitroxModel.Packets;

[Serializable]
public class OpPlayer : Packet
{
    public bool Op;

    public OpPlayer(bool op)
    {
        Op = op;
    }
}
