using System;
using NitroxModel.DataStructures.Unity;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class CameraMovementData
    {
        [ProtoMember(1)]
        public NitroxTechType TechType { get; }

        [ProtoMember(2)]
        public NitroxId Id { get; set; }

        [ProtoMember(3)]
        public NitroxVector3 Position { get; }

        [ProtoMember(4)]
        public NitroxQuaternion Rotation { get; }

        protected CameraMovementData()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public CameraMovementData(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation)
        {
            TechType = techType;
            Id = id;
            Position = position;
            Rotation = rotation;
        }

        public override string ToString()
        {
            return $"[CameraMovementData - TechType: {TechType}, Id: {Id}, Position: {Position}, Rotation: {Rotation}]";
        }
    }
}
