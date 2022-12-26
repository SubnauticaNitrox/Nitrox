namespace NitroxServer.Resources;

public class PrefabPlaceholdersGroupAsset
{
    /// <summary>
    /// All attached PrefabPlaceholders by their classId. Is in sync with PrefabPlaceholdersGroup.prefabPlaceholders
    /// </summary>
    public string[] PrefabPlaceholders { get; }

    public PrefabPlaceholdersGroupAsset(string[] prefabPlaceholders)
    {
        PrefabPlaceholders = prefabPlaceholders;
    }
}
