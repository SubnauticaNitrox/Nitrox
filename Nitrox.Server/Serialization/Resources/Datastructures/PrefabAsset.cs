using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Server.GameLogic.Entities;

namespace Nitrox.Server.Serialization.Resources.Datastructures
{
    public class PrefabAsset
    {
        public string Name { get; set; }
        public string ClassId { get; set; }
        public TransformAsset TransformAsset { get; set; }
        public Optional<NitroxEntitySlot> EntitySlot { get; set; }
        public List<PrefabAsset> Children { get; set; }

        public PrefabAsset(string name, string classId, TransformAsset transformAsset, Optional<NitroxEntitySlot> entitySlot, List<PrefabAsset> children)
        {
            Name = name;
            ClassId = classId;
            TransformAsset = transformAsset;
            EntitySlot = entitySlot;
            Children = children;
        }
    }
}
