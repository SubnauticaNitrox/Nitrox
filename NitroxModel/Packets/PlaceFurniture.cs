using NitroxModel.DataStructures;
using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlaceFurniture : PlayerActionPacket
    {
        public String Guid { get; }
        public String SubGuid { get; }
        public Vector3 ItemPosition { get { return serializableItemPosition.ToVector3(); } }
        public Quaternion Rotation { get { return serializableRotation.ToQuaternion(); } }
        public TechType TechType { get { return serializableTechType.TechType; } }

        private SerializableVector3 serializableItemPosition;
        private SerializableQuaternion serializableRotation;
        private SerializableTransform serializableCamera;
        private SerializableTechType serializableTechType;

        public PlaceFurniture(String playerId, String guid, String subGuid, Vector3 itemPosition, Quaternion rotation, Transform camera, TechType techType) : base(playerId, itemPosition)
        {
            this.Guid = guid;
            this.SubGuid = subGuid;
            this.serializableItemPosition = SerializableVector3.From(itemPosition);
            this.serializableRotation = SerializableQuaternion.From(rotation);
            this.serializableCamera = SerializableTransform.From(camera);
            this.serializableTechType = new SerializableTechType(techType);
        }

        public void CopyCameraTransform(Transform transform)
        {
            serializableCamera.SetTransform(transform);
        }

        public override string ToString()
        {
            return "[PlaceFurniture - ItemPosition: " + serializableItemPosition + " Guid: " + Guid + " SubGuid: " + SubGuid + " Rotation: " + serializableRotation + " Camera: " + serializableCamera + " TechType: " + TechType + "]";
        }
    }
}
