using System;
using System.Numerics;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using ProtoBufNet;

namespace NitroxModel.DataStructures.Unity
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

        public Matrix4x4 LocalToWorldMatrix
        {
            get
            {
                Matrix4x4 cachedMatrix = Matrix4x4Extension.Compose(LocalPosition, LocalRotation, LocalScale);

                return Parent != null ? cachedMatrix * Parent.LocalToWorldMatrix : cachedMatrix;
            }
        }

        public NitroxTransform Parent;
        public Entity Entity;

        public NitroxVector3 Position
        {
            get
            {
                Matrix4x4 matrix = Parent?.LocalToWorldMatrix ?? Matrix4x4.Identity;
                return matrix.Transform(LocalPosition);
            }
            set
            {
                Matrix4x4 matrix = Parent != null ? Matrix4x4Extension.Compose(value, LocalRotation, LocalScale) * Parent.LocalToWorldMatrix.Invert() : Matrix4x4Extension.Compose(value, LocalRotation, LocalScale);
                LocalPosition = (NitroxVector3)matrix.Translation;
            }
        }
        public NitroxQuaternion Rotation
        {
            get
            {
                Matrix4x4 matrix = Parent?.LocalToWorldMatrix ?? Matrix4x4.Identity;
                Matrix4x4.Decompose(matrix, out Vector3 _, out Quaternion rotation, out Vector3 _);

                return (NitroxQuaternion)rotation * LocalRotation;
            }
            set
            {
                Matrix4x4 matrix = Parent != null ? Matrix4x4Extension.Compose(LocalPosition, value, LocalScale) * Parent.LocalToWorldMatrix.Invert() : Matrix4x4Extension.Compose(LocalPosition, value, LocalScale);
                Matrix4x4.Decompose(matrix, out Vector3 _, out Quaternion rotation, out Vector3 _);

                LocalRotation = (NitroxQuaternion)rotation;
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
        { }

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
            return $"(Position: {Position}, LocalPosition: {LocalPosition}, Rotation: {Rotation}, LocalRotation: {LocalRotation}, LocalScale: {LocalScale})";
        }
    }
}
