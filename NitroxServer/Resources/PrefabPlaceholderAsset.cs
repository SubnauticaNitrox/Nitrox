using NitroxModel.DataStructures.Unity;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Resources;

public record struct PrefabPlaceholderAsset(string ClassId, NitroxEntitySlot EntitySlot = null, NitroxTransform Transform = null) : IPrefabAsset
{
    public NitroxTransform Transform { get; set; } = Transform;
    /// <summary>
    /// Some PrefabPlaceholders spawn GameObjects that are always there (decor, environment ...)
    /// And some others spawn a GameObject with an EntitySlot in which case this field is not null.
    /// </summary>
    public NitroxEntitySlot EntitySlot { get; } = EntitySlot;
}
