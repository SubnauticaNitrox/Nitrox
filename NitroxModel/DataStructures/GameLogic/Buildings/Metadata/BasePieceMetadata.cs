using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic.Buildings.Metadata
{
    [DynamicUnion]
    [ProtoContract, ProtoInclude(50, typeof(SignMetadata))]
    public abstract class BasePieceMetadata
    {
    }
}
