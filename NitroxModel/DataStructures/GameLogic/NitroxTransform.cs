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

        public NitroxMatrix4x4 localToWorldMatrix 
        {
            get
            {
                NitroxMatrix4x4 localMatrix = NitroxMatrix4x4.TRS(LocalPosition, LocalRotation, LocalScale);
                return Parent != null ? Parent.localToWorldMatrix * localMatrix : localMatrix;
            }
        }

        public NitroxTransform Parent;
        public Entity Entity;
        public NitroxVector3 Position;
        public NitroxQuaternion Rotation;

        public void SetParent(NitroxTransform parent)
        {
            Parent = parent;
            NitroxVector3 _;
            NitroxMatrix4x4 local2WorldMatrix = localToWorldMatrix;
            NitroxMatrix4x4.DecomposeMatrix(ref local2WorldMatrix, out Position, out Rotation, out _);
            
        }

        public void SetParent(NitroxTransform parent, bool worldPositionStays)
        {
            throw new NotImplementedException("This is not Implementwaed yet. Added by killzoms");
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
