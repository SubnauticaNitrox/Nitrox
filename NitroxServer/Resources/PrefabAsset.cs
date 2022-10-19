using System.Collections.Generic;
using NitroxModel.DataStructures.Util;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Resources
{
    public class PrefabAsset
    {
        public string Name { get; }
        public string ClassId { get; }
        public TransformAsset TransformAsset { get; }
        public List<PrefabAsset> Children { get; }
        public Optional<NitroxEntitySlot> EntitySlot { get; }
        public Optional<string> PlaceholderIdentifier { get; }

        public PrefabAsset(string name, string classId, TransformAsset transformAsset, List<PrefabAsset> children, Optional<NitroxEntitySlot> entitySlot, Optional<string> placeholderIdentifier)
        {
            Name = name;
            ClassId = classId;
            TransformAsset = transformAsset;
            Children = children;
            EntitySlot = entitySlot;
            PlaceholderIdentifier = placeholderIdentifier;
        }
    }
}
