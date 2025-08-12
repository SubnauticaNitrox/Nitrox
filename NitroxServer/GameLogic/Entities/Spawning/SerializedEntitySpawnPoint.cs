using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Unity;

namespace NitroxServer.GameLogic.Entities.Spawning;

/// <summary>
/// Specific type of <see cref="EntitySpawnPoint"/> for spawning <see cref="SerializedWorldEntity"/>
/// </summary>
public class SerializedEntitySpawnPoint : EntitySpawnPoint
{
    public List<SerializedComponent> SerializedComponents { get; }
    public int Layer { get; }

    public SerializedEntitySpawnPoint(List<SerializedComponent> serializedComponents, int layer, AbsoluteEntityCell absoluteEntityCell, NitroxTransform transform) : base(absoluteEntityCell, transform.LocalPosition, transform.LocalRotation, null, 1, null)
    {
        SerializedComponents = serializedComponents;
        Layer = layer;
        Scale = transform.LocalScale;
    }
}
