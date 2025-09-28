using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class FabricatorStateChanged : Packet
    {
        public NitroxId Id { get; }
        public ushort PlayerId { get; }
        public bool IsCrafting { get; }

        public FabricatorStateChanged(NitroxId id, ushort playerId, bool isCrafting)
        {
            Id = id;
            PlayerId = playerId;
            IsCrafting = isCrafting;
        }
    }
}
