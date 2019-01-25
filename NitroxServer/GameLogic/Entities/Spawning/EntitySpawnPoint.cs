using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using UnityEngine;

namespace NitroxServer.GameLogic.Entities.Spawning
{
    public class EntitySpawnPoint
    {
        public AbsoluteEntityCell AbsoluteEntityCell { get; private set; }
        public Vector3 LocalPosition { get; private set; }
        public Quaternion LocalRotation { get; private set; }
        public Vector3 LocalScale { get; private set; }
        public string ClassId { get; private set; }
        public string BiomeType { get; private set; }
        public float Density { get; private set; }
        public bool CanSpawnCreature { get; private set; }
        public List<string> AllowedTypes { get; private set; }
        public EntitySpawnPoint Parent { get; private set; }

        public List<EntitySpawnPoint> Children = new List<EntitySpawnPoint>();

        private static Dictionary<string, EntitySpawnPoint> parentsById = new Dictionary<string, EntitySpawnPoint>();
        
        public EntitySpawnPoint(AbsoluteEntityCell absoluteEntityCell, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, string classId, string id, string parentId)
        {
            AbsoluteEntityCell = absoluteEntityCell;
            ClassId = classId;
            Density = 1;
            LocalPosition = localPosition;
            LocalScale = localScale;
            LocalRotation = localRotation;

            EntitySpawnPoint parent = null;
            if (parentId == null)
            {
                parentsById.Add(id, this);
            }
            else
            {
                parentsById.TryGetValue(parentId, out parent);
            }

            Parent = parent;

            if (Parent != null)
            {
                Parent.Children.Add(this);
            }
        }

        public EntitySpawnPoint(AbsoluteEntityCell absoluteEntityCell, Vector3 localPosition, Quaternion localRotation, List<string> allowedTypes, float density, string biomeType, string id, string parentId)
        {
            AbsoluteEntityCell = absoluteEntityCell;
            Density = density;
            AllowedTypes = allowedTypes;
            BiomeType = biomeType;
            LocalPosition = localPosition;
            LocalRotation = localRotation;

            EntitySpawnPoint parent = null;
            if (parentId == null)
            {
                parentsById.Add(id, this);
            }
            else
            {
                parentsById.TryGetValue(parentId, out parent);
            }

            Parent = parent;

            if (Parent != null)
            {
                Parent.Children.Add(this);
            }
        }

        public override string ToString() => $"[EntitySpawnPoint - {AbsoluteEntityCell}, Position: {LocalPosition}, Rotation: {LocalRotation}, Scale: {LocalScale}, ClassId: {ClassId}, BiomeType: {BiomeType}, Density: {Density}, CanSpawnCreature: {CanSpawnCreature}]";
    }
}
