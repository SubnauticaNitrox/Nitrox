using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerTeleported : Packet
    {
        public string PlayerName { get; }
        public NitroxVector3 DestinationFrom { get; }
        public NitroxVector3 DestinationTo { get; }

        public PlayerTeleported(string playerName, NitroxVector3 destinationFrom, NitroxVector3 destinationTo)
        {
            PlayerName = playerName;
            DestinationFrom = destinationFrom;
            DestinationTo = destinationTo;
        }
    }
}
