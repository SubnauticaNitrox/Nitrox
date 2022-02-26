namespace NitroxModel.DataStructures.GameLogic.Entities;

public class UwePrefab
{
    public string ClassId { get; }
    public float Probability { get; set; }
    public int Count { get; }
    public string PrefabPath { get; }

    public UwePrefab(string classId, float probability, int count, string prefabPath)
    {
        ClassId = classId;
        Probability = probability;
        Count = count;
        PrefabPath = prefabPath;
    }
}
