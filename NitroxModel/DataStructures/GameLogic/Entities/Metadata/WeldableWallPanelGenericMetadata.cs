using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [ProtoContract]
    public class WeldableWallPanelGenericMetadata : EntityMetadata
    {
        [ProtoMember(1)]
        public float LiveMixInHealth { get; }

        protected WeldableWallPanelGenericMetadata()
        {
            //Constructor for serialization. Has to be "protected" for json serialization.
        }

        public WeldableWallPanelGenericMetadata(float liveMixInHealth)
        {
            LiveMixInHealth = liveMixInHealth;
        }

        public override string ToString()
        {
            return $"[WeldableWallPanelGenericMetadata LiveMixInHealth: {LiveMixInHealth}]";
        }
    }
}
