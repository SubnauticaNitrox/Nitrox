using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EntityReparented : Packet
    {
        //make sure to update the parent
        public NitroxId Id { get; }

        public NitroxId NewParentId { get; }

        public EntityReparented(NitroxId id, NitroxId newParentId)
        {
            Id = id;
            NewParentId = newParentId;
        }
        public override string ToString()
        {
            return $"[EntityReparented Id: {Id} NewParentId: {NewParentId}]";
        }
    }
}
