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
                Matrix4x4 cachedMatrix = Matrix4x4Extension.Compose(LocalPosition, LocalRotation, LocalScale); // its not cached yet

                return Parent != null ? cachedMatrix * Parent.LocalToWorldMatrix : cachedMatrix;
            }
        }

        public NitroxTransform Parent;

        [IgnoredMember]
        public NitroxVector3 Position
        {
            get
            {
                return TransformPoint(LocalPosition);
            }
            set
            {
                LocalPosition = InverseTransformPoint(value);
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
                Matrix4x4 matrix = Parent != null ? Matrix4x4.CreateFromQuaternion((Quaternion)value) * Parent.LocalToWorldMatrix.Invert() : Matrix4x4Extension.Compose(LocalPosition, value, LocalScale);
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

        public NitroxVector3 InverseTransformPoint(NitroxVector3 point)
        {
            Matrix4x4 pointMatrix = Parent != null ? Matrix4x4.CreateTranslation((Vector3)point) * Parent.LocalToWorldMatrix.Invert() : Matrix4x4.CreateTranslation((Vector3)point);

            return (NitroxVector3)pointMatrix.Translation;
        }

        public NitroxVector3 TransformPoint(NitroxVector3 localPoint)
        {
            Matrix4x4 pointMatrix = Parent != null ? Matrix4x4.CreateTranslation((Vector3)localPoint) * Parent.LocalToWorldMatrix : Matrix4x4.CreateTranslation((Vector3)localPoint);

            return (NitroxVector3)pointMatrix.Translation;
        }

        public override string ToString()
        {
            return $"(Position: {Position}, LocalPosition: {LocalPosition}, Rotation: {Rotation}, LocalRotation: {LocalRotation}, LocalScale: {LocalScale})";
        }
    }
}
