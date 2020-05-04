using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerTeleported : Packet
    {
        public string PlayerName { get; }
        public Vector3 DestinationFrom { get; }
        public Vector3 DestinationTo { get; }

        public PlayerTeleported(string playerName, Vector3 destinationFrom, Vector3 destinationTo)
        {
            PlayerName = playerName;
            DestinationFrom = destinationFrom;
            DestinationTo = destinationTo;
        }
    }
}
