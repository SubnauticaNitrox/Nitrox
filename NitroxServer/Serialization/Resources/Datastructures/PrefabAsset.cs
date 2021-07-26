using NitroxModel.DataStructures.Util;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Serialization.Resources.Datastructures
{
    public class PrefabAsset
    {
        public string ClassId { get; set; }
        public TransformAsset TransformAsset { get; set; }
        public Optional<NitroxEntitySlot> EntitySlot { get; set; }

        public PrefabAsset(string classId, TransformAsset transformAsset, Optional<NitroxEntitySlot> entitySlot)
        {
            ClassId = classId;
            TransformAsset = transformAsset;
            EntitySlot = entitySlot;
        }
    }
}
