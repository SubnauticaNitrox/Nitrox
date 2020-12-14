using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class EntityTransformUpdates : Packet
    {
        public List<EntityTransformUpdate> Updates { get; }

        public EntityTransformUpdates()
        {
            Updates = new List<EntityTransformUpdate>();
        }

        public EntityTransformUpdates(List<EntityTransformUpdate> updates)
        {
            Updates = updates;
        }

        public void AddUpdate(NitroxId id, NitroxVector3 position, NitroxQuaternion rotation)
        {
            Updates.Add(new EntityTransformUpdate(id, position, rotation));
        }

        public override string ToString()
        {
            return $"[EntityTransformUpdates - Updates: {Updates?.Count}]";
        }

        public override string ToLongString()
        {
            return $"[EntityTransformUpdates - Updates: ({string.Join(", ", Updates)})]";
        }
    }
}
