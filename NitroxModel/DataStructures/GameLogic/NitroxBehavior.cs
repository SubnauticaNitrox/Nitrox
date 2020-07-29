using System;
using System.Runtime.Serialization;
using NitroxModel.Logger;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    [ProtoContract]
    [ProtoInclude(100, typeof(NitroxTransform))]
    [ProtoInclude(200, typeof(Entity))]
    [ProtoInclude(300, typeof(BasePiece))]
    [ProtoInclude(400, typeof(EscapePodModel))]
    [ProtoInclude(500, typeof(VehicleModel))]
    [Serializable]
    public class NitroxBehavior : ISerializable
    {
        public NitroxObject NitroxObject { get; internal set; }
        public NitroxTransform Transform => NitroxObject.Transform;

        public NitroxVector3 Position
        {
            get
            {
                return Transform.Position;
            }
            set
            {
                Transform.Position = value;
            }
        }

        public NitroxQuaternion Rotation
        {
            get
            {
                return Transform.Rotation;
            }
            set
            {
                Transform.Rotation = value;
            }
        }

        public NitroxVector3 LocalScale
        {
            get
            {
                return Transform.LocalScale;
            }
            set
            {
                Transform.LocalScale = value;
            }
        }


        public NitroxId Id => NitroxObject.Id;

        protected NitroxBehavior()
        {
            // Proto
        }

        protected NitroxBehavior(SerializationInfo info, StreamingContext context)
        {
            NitroxObject = (NitroxObject)info.GetValue("object", typeof(NitroxObject));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("object", NitroxObject, typeof(NitroxObject));
        }
    }
}
