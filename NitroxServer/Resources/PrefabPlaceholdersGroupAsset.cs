namespace NitroxServer.Resources;

public class PrefabPlaceholdersGroupAsset
{
    /// <summary>
    /// All PrefabPlaceholders by their classId
    /// </summary>
    public string[] PrefabPlaceholders { get; }

    public PrefabPlaceholdersGroupAsset(string[] prefabPlaceholders)
    {
        PrefabPlaceholders = prefabPlaceholders;
    }
}
