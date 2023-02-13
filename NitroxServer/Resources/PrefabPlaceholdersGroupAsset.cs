using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.Resources;

public class PrefabPlaceholdersGroupAsset
{
    /// <summary>
    /// All attached PrefabPlaceholders. Is in sync with PrefabPlaceholdersGroup.prefabPlaceholders
    /// </summary>
    public PrefabPlaceholderAsset[] PrefabPlaceholders { get; }

    public NitroxTechType TechType { get;}

    public PrefabPlaceholdersGroupAsset(PrefabPlaceholderAsset[] prefabPlaceholders, NitroxTechType techType)
    {
        PrefabPlaceholders = prefabPlaceholders;
        TechType = techType;
    }
}
