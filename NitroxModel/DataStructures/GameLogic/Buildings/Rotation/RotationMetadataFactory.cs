﻿
using NitroxModel.DataStructures.Util;

namespace NitroxModel.DataStructures.GameLogic.Buildings.Rotation
{
    public interface RotationMetadataFactory
    {
        Optional<RotationMetadata> From(object o);
    }
}
