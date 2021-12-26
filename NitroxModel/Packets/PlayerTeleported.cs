using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class PlayerTeleported : Packet
    {
        [Index(0)]
        public virtual string PlayerName { get; protected set; }
        [Index(1)]
        public virtual NitroxVector3 DestinationFrom { get; protected set; }
        [Index(2)]
        public virtual NitroxVector3 DestinationTo { get; protected set; }
        [Index(3)]
        public virtual Optional<NitroxId> SubRootID { get; protected set; }

        private PlayerTeleported() { }

        public PlayerTeleported(string playerName, NitroxVector3 destinationFrom, NitroxVector3 destinationTo, Optional<NitroxId> subRootID)
        {
            PlayerName = playerName;
            DestinationFrom = destinationFrom;
            DestinationTo = destinationTo;
            SubRootID = subRootID;
        }
    }
}
