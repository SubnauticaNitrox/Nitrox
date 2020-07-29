using System;
using System.Runtime.Serialization;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    [ProtoContract]
    [Serializable]
    public sealed class NitroxTransform : NitroxBehavior
    {
        public NitroxMatrix4x4 localToWorldMatrix
        {
            get
            {
                NitroxMatrix4x4 localMatrix = NitroxMatrix4x4.TRS(LocalPosition, LocalRotation, LocalScale);
                return Parent != null ? Parent.localToWorldMatrix * localMatrix : localMatrix;
            }
        }

        [ProtoMember(4)]
        private NitroxId ParentId;

        public NitroxVector3 Position
        {
            get
            {
                NitroxMatrix4x4 matrix = Parent != null ? Parent.localToWorldMatrix : NitroxMatrix4x4.Identity;
                return matrix.MultiplyPoint(LocalPosition);
            }
            set
            {
                NitroxMatrix4x4 matrix = Parent != null ? Parent.localToWorldMatrix.Inverse * NitroxMatrix4x4.TRS(value, LocalRotation, LocalScale) : NitroxMatrix4x4.TRS(value, LocalRotation, LocalScale);

                LocalPosition = NitroxMatrix4x4.ExtractTranslation(ref matrix);
            }
        }
        public NitroxQuaternion Rotation
        {
            get
            {
                NitroxMatrix4x4 matrix = Parent != null ? Parent.localToWorldMatrix : NitroxMatrix4x4.Identity;
                NitroxMatrix4x4.ExtractScale(ref matrix); // This is to just get the scale out of the matrix so the rotation is accurate
                NitroxQuaternion rotation = NitroxMatrix4x4.ExtractRotation(ref matrix) * LocalRotation;
                return rotation;
            }
            set
            {
                NitroxMatrix4x4 matrix = Parent != null ? Parent.localToWorldMatrix.Inverse * NitroxMatrix4x4.TRS(LocalPosition, value, LocalScale) : NitroxMatrix4x4.TRS(LocalPosition, value, LocalScale);

                NitroxMatrix4x4.ExtractScale(ref matrix);
                LocalRotation = NitroxMatrix4x4.ExtractRotation(ref matrix);
            }
        }

        [ProtoMember(1)]
        public NitroxVector3 LocalPosition { get; set; }
        [ProtoMember(2)]
        public NitroxQuaternion LocalRotation { get; set; }
        [ProtoMember(3)]
        public NitroxVector3 LocalScale { get; set; }

        public NitroxTransform Parent { get; private set; }

        public void SetParent(NitroxTransform parent)
        {
            if (parent == null)
            {
                return;
            }

            if (Parent != null)
            {
                parent.NitroxObject.Children.Remove(NitroxObject);
            }

            Parent = parent;
            ParentId = parent.Id;

            parent.NitroxObject.Children.Add(NitroxObject);

        }

        public void SetParent(NitroxTransform parent, bool worldPositionStays)
        {
            NitroxVector3 curPos = Position;

            SetParent(parent);

            if (worldPositionStays)
            {
                Position = curPos;
            }
        }

        private NitroxTransform()
        {}

        /// <summary>
        /// NitroxTransform is always attached to an Object
        /// </summary>
        internal NitroxTransform(NitroxVector3 localPosition, NitroxQuaternion localRotation, NitroxVector3 scale)
        {
            LocalPosition = localPosition;
            LocalRotation = localRotation;
            LocalScale = scale;
        }

        private NitroxTransform(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ParentId = (NitroxId)info.GetValue("parent", typeof(NitroxId));
            LocalPosition = (NitroxVector3)info.GetValue("localPosition", typeof(NitroxVector3));
            LocalRotation = (NitroxQuaternion)info.GetValue("localRotation", typeof(NitroxQuaternion));
            LocalScale = (NitroxVector3)info.GetValue("localScale", typeof(NitroxVector3));
            if (ParentId != null)
            {
                SetParent(NitroxObject.GetObjectById(ParentId)?.Transform);
            }
        }

        [ProtoAfterDeserialization]
        private void AfterDeserialization()
        {
            if (ParentId != null)
            {
                SetParent(NitroxObject.GetObjectById(ParentId)?.Transform);
            }
        }

        public override string ToString()
        {
            return string.Format("(Position: {0}, LocalPosition: {1}, Rotation: {2}, LocalRotation: {3}, LocalScale: {4})", Position, LocalPosition, Rotation, LocalRotation, LocalScale);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("parent", ParentId);
            info.AddValue("localPosition", LocalPosition);
            info.AddValue("localRotation", LocalRotation);
            info.AddValue("localScale", LocalScale);
        }
    }
}
