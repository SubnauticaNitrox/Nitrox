namespace NitroxModel.DataStructures.GameLogic.Entities;

public record struct UwePrefab(string ClassId, int Count, float Probability, bool IsFragment)
{
    public string ClassId { get; } = ClassId;
    public int Count { get; } = Count;
    public float Probability { get; set; } = Probability;
    public bool IsFragment { get; } = IsFragment;
}
