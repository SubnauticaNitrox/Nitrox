using UnityEngine;

namespace NitroxModel.DataStructures.GameLogic.Entities
{
    public class UwePrefab
    {
        public string ClassId { get; }
        public float Probability { get; }
        public int Count { get; }
        
        public Vector3 LocalPosition { get; }
        public Quaternion LocalRotation { get; }
        public Vector3 LocalScale { get; }

        public string Guid { get; set; }
        public int CellLevel { get; set; }
        public TechType TechType { get; set; }

        public UwePrefab(string classId, float probability, int count)
        {
            ClassId = classId;
            Probability = probability;
            Count = count;
        }

        public UwePrefab(string classId, Vector3 localPosition, Quaternion localRotation, Vector3 scale)
        {
            ClassId = classId;
            LocalPosition = localPosition;
            LocalRotation = localRotation;
            LocalScale = scale;
        }
    }
}
