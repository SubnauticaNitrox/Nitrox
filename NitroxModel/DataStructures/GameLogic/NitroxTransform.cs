using System;
using ProtoBufNet;

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
        public NitroxVector3 Position
        {
            get 
            {
                NitroxMatrix4x4 matrix = Parent?.localToWorldMatrix ?? NitroxMatrix4x4.Identity;
                return matrix.MultiplyPoint(LocalPosition);
            }
            set
            {
                NitroxMatrix4x4 matrix = Parent != null ? Parent.localToWorldMatrix.Inverse * NitroxMatrix4x4.TRS(value, LocalRotation, LocalScale) : NitroxMatrix4x4.TRS(value, LocalRotation, LocalScale);

                LocalPosition = NitroxMatrix4x4.GetTranslation(ref matrix);
            }
        }
        public NitroxQuaternion Rotation
        {
            get
            {
                NitroxMatrix4x4 matrix = Parent?.localToWorldMatrix ?? NitroxMatrix4x4.Identity;
                NitroxMatrix4x4 rMatrix = NitroxMatrix4x4.ExtractScale(ref matrix, out _);
                NitroxQuaternion rotation = NitroxMatrix4x4.GetRotation(ref rMatrix) * LocalRotation;
                return rotation;
            }
            set
            {
                NitroxMatrix4x4 matrix = Parent != null ? Parent.localToWorldMatrix.Inverse * NitroxMatrix4x4.TRS(LocalPosition, value, LocalScale) : NitroxMatrix4x4.TRS(LocalPosition, value, LocalScale);
                NitroxMatrix4x4 rMatrix = NitroxMatrix4x4.ExtractScale(ref matrix, out _);
                LocalRotation = NitroxMatrix4x4.GetRotation(ref rMatrix);
            }
        }

        public void SetParent(NitroxTransform parent, bool worldPositionStays = true)
        {
            if (!worldPositionStays)
            {
                Parent = parent;
                return;
            }

            NitroxVector3 position = Position;
            NitroxQuaternion rotation = Rotation;

            Parent = parent;

            Position = position;
            Rotation = rotation;
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
            return $"[Transform - Position: {Position}; LocalPosition: {LocalPosition}; Rotation: {Rotation}; LocalRotation: {LocalRotation}; LocalScale: {LocalScale}]";
        }
    }
}
