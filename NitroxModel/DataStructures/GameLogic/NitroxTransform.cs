using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
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

        [ProtoMember(4)]
        private NitroxId parentId;

        [ProtoMember(5)]
        public NitroxId Id;

        private static Dictionary<NitroxId, NitroxTransform> transformsById = new Dictionary<NitroxId, NitroxTransform>();
        private static Dictionary<NitroxId, List<NitroxTransform>> pendingTransforms = new Dictionary<NitroxId, List<NitroxTransform>>();

        public NitroxMatrix4x4 localToWorldMatrix 
        {
            get
            {
                NitroxMatrix4x4 localMatrix = NitroxMatrix4x4.TRS(LocalPosition, LocalRotation, LocalScale);
                return Parent != null ? Parent.localToWorldMatrix * localMatrix : localMatrix;
            }
        }

        public NitroxTransform Parent { get; private set; }

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

        public void SetParent(NitroxTransform parent)
        {
            Parent = parent;
            parentId = parent.Id;
        }

        public void SetParent(NitroxTransform parent, bool worldPositionStays)
        {
            NitroxVector3 oldPostion = Position;
            SetParent(parent);
            if (worldPositionStays)
            {
                Position = oldPostion;
            }
        }

        public NitroxVector3 TransformPoint(NitroxVector3 point)
        {
            return localToWorldMatrix.MultiplyPoint(point);
        }

        public NitroxVector3 InverseTransformPoint(NitroxVector3 point)
        {
            return localToWorldMatrix.Inverse.MultiplyPoint(point);
        }

        private NitroxTransform()
        {}

        public NitroxTransform(NitroxVector3 localPosition, NitroxQuaternion localRotation, NitroxVector3 scale, NitroxId id)
        {
            LocalPosition = localPosition;
            LocalRotation = localRotation;
            LocalScale = scale;
            Id = id;
        }

        private void HandleParenting()
        {
            if (pendingTransforms.TryGetValue(Id, out List<NitroxTransform> transforms))
            {
                foreach (NitroxTransform transform in transforms)
                {
                    transform.SetParent(this);
                }
                pendingTransforms.Remove(Id);
            }

            if (parentId != null && transformsById.TryGetValue(parentId, out NitroxTransform parentTransform))
            {
                SetParent(parentTransform);
            }
            else if (parentId != null)
            {
                if (!pendingTransforms.TryGetValue(parentId, out List<NitroxTransform> parentTransforms))
                {
                    parentTransforms = new List<NitroxTransform>
                    {
                        this
                    };

                    pendingTransforms.Add(parentId, parentTransforms);
                }
                else
                {
                    pendingTransforms[parentId].Add(this);
                }
            }

            transformsById[Id] = this;
        }


        [ProtoAfterDeserialization]
        private void ProtoAfterDeserialization()
        {
            if (Id != null)
            {
                HandleParenting();
            }
        }

        [OnDeserialized]
        private void JsonAfterDeserialization(StreamingContext context)
        {
            ProtoAfterDeserialization();
        }

        public override string ToString()
        {
            return string.Format("(Position: {0}, LocalPosition: {1}, Rotation: {2}, LocalRotation: {3}, LocalScale: {4})", Position, LocalPosition, Rotation, LocalRotation, LocalScale);
        }
    }
}
