using System;
using UnityEngine;
using ProtoBufNet;
using System.Collections.Generic;

namespace NitroxModel.DataStructures.GameLogic
{
    [ProtoContract]
    [Serializable]
    public class NitroxTransform
    {
        [ProtoMember(1)]
        public NitroxVector3 LocalPosition;

        [ProtoMember(2)]
        public NitroxQuaternion LocalRotation;
        
        [ProtoMember(3)]
        public NitroxVector3 LocalScale;

        public NitroxTransform Parent;
        public Entity Entity;
        public NitroxVector3 Position => Parent != null ? Parent.Position + LocalPosition : LocalPosition;
        public NitroxQuaternion Rotation => Parent != null ? Parent.Rotation * LocalRotation : LocalRotation;

        public void SetParent(NitroxTransform parent)
        {
            Parent = parent;
        }

        public void SetParent(NitroxTransform parent, bool worldPositionStays)
        {
            throw new NotImplementedException("This is not Implemented yet. Added by killzoms");
        }

        private NitroxTransform()
        {}

        /// <summary>
        /// NitroxTransform is always attached to an Entity
        /// </summary>
        public NitroxTransform(NitroxVector3 localPosition, NitroxQuaternion localRotation, NitroxVector3 scale, Entity entity)
        {
            LocalPosition = localPosition;
            LocalRotation = localRotation;
            LocalScale = scale;
            Entity = entity;
        }

        public override string ToString()
        {
            return string.Format("(Position: {0}, LocalPosition: {1}, Rotation: {2}, LocalRotation: {3}, LocalScale: {4})", Position, LocalPosition, Rotation, LocalRotation, LocalScale);
        }
    }
}
