using System;
using System.Numerics;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.Helper;

namespace NitroxModel.DataStructures.Unity
{
    [DataContract]
    [Serializable]
    public class NitroxTransform
    {
        [DataMember(Order = 1)]
        public NitroxVector3 LocalPosition;

        [DataMember(Order = 2)]
        public NitroxQuaternion LocalRotation;

        [DataMember(Order = 3)]
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

        [IgnoredMember]
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

        [IgnoredMember]
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

        public NitroxTransform() { }

        public NitroxTransform(NitroxVector3 localPosition, NitroxQuaternion localRotation, NitroxVector3 scale)
        {
            LocalPosition = localPosition;
            LocalRotation = localRotation;
            LocalScale = scale;
        }

        public override string ToString()
        {
            return $"(Position: {Position}, LocalPosition: {LocalPosition}, Rotation: {Rotation}, LocalRotation: {LocalRotation}, LocalScale: {LocalScale})";
        }
    }
}
