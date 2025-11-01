using System;
using NitroxModel.Networking;

namespace NitroxModel.Packets;

[Serializable]
public class CheatCommand : Packet
{
    public string Command { get; }

    public CheatCommand(string command)
    {
        Command = command;
        DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.RELIABLE_UNORDERED;
    }
}
