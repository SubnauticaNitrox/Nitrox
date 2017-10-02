using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlaceBasePiece : PlayerActionPacket
    {
        public String Guid { get; }
        public Vector3 ItemPosition { get { return serializableItemPosition.ToVector3(); } }
        public Quaternion Rotation { get { return serializableRotation.ToQuaternion(); } }
        public TechType TechType { get { return serializableTechType.TechType; } }
        public Optional<String> ParentBaseGuid { get; }
        
        private SerializableVector3 serializableItemPosition;
        private SerializableQuaternion serializableRotation;
        private SerializableTransform serializableCamera;
        private SerializableTechType serializableTechType;

        public PlaceBasePiece(String playerId, String guid, Vector3 itemPosition, Quaternion rotation, Transform camera, TechType techType, Optional<String> parentBaseGuid) : base(playerId, itemPosition)
        {
            this.Guid = guid;
            this.serializableItemPosition = SerializableVector3.From(itemPosition);
            this.serializableRotation = SerializableQuaternion.From(rotation);
            this.serializableTechType = new SerializableTechType(techType);
            this.serializableCamera = SerializableTransform.From(camera);
            this.ParentBaseGuid = parentBaseGuid;
        }

        public void CopyCameraTransform(Transform transform)
        {
            serializableCamera.SetTransform(transform);
        }

        public override string ToString()
        {
            return "[PlaceBasePiece - ItemPosition: " + serializableItemPosition + " Guid: " + Guid + " Rotation: " + serializableRotation + " Camera: " + serializableCamera + " TechType: " + TechType + " ParentBaseGuid: " + ParentBaseGuid + "]";
        }
    }
}
