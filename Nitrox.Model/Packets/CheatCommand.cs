using System;
using Nitrox.Model.Networking;

namespace Nitrox.Model.Packets;

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
