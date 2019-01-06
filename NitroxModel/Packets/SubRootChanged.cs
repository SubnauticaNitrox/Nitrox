using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SubRootChanged : Packet
    {
        public PlayerContext PlayerContext;
        public Optional<string> SubRootGuid { get; }

        public SubRootChanged(PlayerContext playerContext, Optional<string> subRootGuid)
        {
            PlayerContext = playerContext;
            SubRootGuid = subRootGuid;
        }

        public override string ToString()
        {
            return "[SubRootChanged - PlayerId: " + PlayerContext.PlayerId + " SubRootGuid: " + SubRootGuid + "]";
        }
    }
}
