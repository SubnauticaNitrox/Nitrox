
using NitroxModel.DataStructures.Util;

namespace NitroxModel.DataStructures.GameLogic.Buildings.Rotation
{
    public interface BuilderMetadataFactory
    {
        Optional<BuilderMetadata> From(object o);
    }
}
