namespace NitroxModel.DataStructures.GameLogic.Entities
{
    public class UwePrefab
    {
        public string ClassId { get; }
        public float Probability { get; }
        public int Count { get; }

        public UwePrefab(string classId, float probability, int count)
        {
            ClassId = classId;
            Probability = probability;
            Count = count;
        }
    }
}
