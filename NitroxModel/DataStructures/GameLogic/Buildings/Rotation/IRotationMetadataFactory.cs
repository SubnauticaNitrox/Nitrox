
using NitroxModel.DataStructures.Util;

namespace NitroxModel.DataStructures.GameLogic.Buildings.Rotation
{
    public interface IRotationMetadataFactory
    {
        Optional<RotationMetadata> From(object o);
    }
}
