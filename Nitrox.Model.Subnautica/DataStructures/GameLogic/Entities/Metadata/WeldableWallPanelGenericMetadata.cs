using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [DataContract]
    public class WeldableWallPanelGenericMetadata : EntityMetadata
    {
        [DataMember(Order = 1)]
        public float LiveMixInHealth { get; }

        [IgnoreConstructor]
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
