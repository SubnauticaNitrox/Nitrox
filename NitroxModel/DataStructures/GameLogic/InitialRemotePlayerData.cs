using System.Collections.Generic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic
{
    [ZeroFormattable]
    public class InitialRemotePlayerData
    {
        [Index(0)]
        public virtual PlayerContext PlayerContext { get; set; }
        [Index(1)]
        public virtual NitroxVector3 Position { get; set; }
        [Index(2)]
        public virtual Optional<NitroxId> SubRootId { get; protected set; }
        [Index(3)]
        public virtual List<NitroxTechType> EquippedTechTypes { get; protected set; }

        protected InitialRemotePlayerData()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public InitialRemotePlayerData(PlayerContext playerContext, NitroxVector3 position, Optional<NitroxId> subRootId, List<NitroxTechType> equippedTechTypes)
        {
            PlayerContext = playerContext;
            Position = position;
            SubRootId = subRootId;
            EquippedTechTypes = equippedTechTypes;
        }
    }
}
