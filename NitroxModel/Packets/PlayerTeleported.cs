using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerTeleported : Packet
    {
        public string PlayerName { get; }
        public NitroxVector3 DestinationFrom { get; }
        public NitroxVector3 DestinationTo { get; }
        public Optional<NitroxId> SubRootID { get; }

        public PlayerTeleported(string playerName, NitroxVector3 destinationFrom, NitroxVector3 destinationTo, Optional<NitroxId> subRootID)
        {
            PlayerName = playerName;
            DestinationFrom = destinationFrom;
            DestinationTo = destinationTo;
            SubRootID = subRootID;
        }
    }
}
