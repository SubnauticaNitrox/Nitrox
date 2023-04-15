using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Packets;

[Serializable]
public class EntityTransformUpdates : Packet
{
    public List<EntityTransformUpdate> Updates { get; }

    public EntityTransformUpdates(List<EntityTransformUpdate> updates)
    {
        Updates = updates;
    }

    public override string ToString()
    {
        return $"[EntityTransformUpdates: {String.Join(" ", Updates)} ]";
    }

    [Serializable]
    public abstract class EntityTransformUpdate
    {
        public NitroxId Id { get; }
        public NitroxVector3 Position { get; }
        public NitroxQuaternion Rotation { get; }

        public EntityTransformUpdate(NitroxId id, NitroxVector3 position, NitroxQuaternion rotation)
        {
            Id = id;
            Position = position;
            Rotation = rotation;
        }
    }

    [Serializable]
    public class RawTransformUpdate : EntityTransformUpdate
    {
        public RawTransformUpdate(NitroxId id, NitroxVector3 position, NitroxQuaternion rotation) : base(id, position, rotation)
        {

        }

        public override string ToString()
        {
            return $"[RawTransformUpdate Id:{Id} Position:{Position} Rotation:{Rotation}]";
        }
    }

    [Serializable]
    public class SplineTransformUpdate : EntityTransformUpdate
    {
        public NitroxVector3 DestinationPosition { get; }
        public NitroxVector3 DestinationDirection { get; }
        public float Velocity { get; }

        public SplineTransformUpdate(NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, NitroxVector3 destinationPosition, NitroxVector3 destinationDirection, float velocity) : base(id, position, rotation)
        {
            DestinationPosition = destinationPosition;
            DestinationDirection = destinationDirection;
            Velocity = velocity;
        }

        public override string ToString()
        {
            return $"[SplineTransformUpdate Id:{Id} Position:{Position} Rotation:{Rotation} DestinationPosition:{DestinationPosition} DestinationDirection:{DestinationDirection} Velocity:{Velocity} ]";
        }
    }
}
