using System;
using System.Runtime.Serialization;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic
{
    [Serializable]
    [DataContract]
    public class GenericFireData
    {
        [DataMember(Order = 1)]
        public NitroxId FireId { get; set; }

        [DataMember(Order = 2)]
        public NitroxId ParentId { get; set; }

        [DataMember(Order = 3)]
        public NitroxVector3 Position { get; set; }

        [DataMember(Order = 4)]
        public NitroxQuaternion Rotation { get; set; }

        protected GenericFireData()
        {
        }

        public GenericFireData(NitroxId fireId, NitroxId parentId, NitroxVector3 position, NitroxQuaternion rotation)
        {
            FireId = fireId;
            ParentId = parentId;
            Position = position;
            Rotation = rotation;
        }
    }
}
