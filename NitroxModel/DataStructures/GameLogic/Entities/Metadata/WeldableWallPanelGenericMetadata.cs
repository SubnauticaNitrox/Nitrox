using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [ZeroFormattable]
    [ProtoContract]
    public class WeldableWallPanelGenericMetadata : EntityMetadata
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual float LiveMixInHealth { get; protected set; }

        public WeldableWallPanelGenericMetadata()
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
