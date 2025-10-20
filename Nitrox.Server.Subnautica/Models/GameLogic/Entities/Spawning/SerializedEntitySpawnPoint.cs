using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;

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
