using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using UnityEngine;

namespace NitroxServer.GameLogic.Entities.Spawning
{
    public class EntitySpawnPoint
    {
        public AbsoluteEntityCell AbsoluteEntityCell { get; private set; }
        public Vector3 Position { get { return Parent != null ? Parent.Position + position : position; } }

        private Vector3 position;
        public Quaternion Rotation { get { return Parent != null ? Parent.Rotation * rotation : rotation; } }

        private Quaternion rotation;
        public Vector3 Scale { get; private set; }
        public string ClassId { get; private set; }
        public string BiomeType { get; private set; }
        public float Density { get; private set; }
        public bool CanSpawnCreature { get; private set; }
        public List<string> AllowedTypes { get; private set; }

        public EntitySpawnPoint Parent { get; private set; }

        public readonly List<EntitySpawnPoint> Children = new List<EntitySpawnPoint>();

        public EntitySpawnPoint(AbsoluteEntityCell absoluteEntityCell, Vector3 localPosition, Quaternion localRotation, List<string> allowedTypes, float density, string biomeType)
        {
            AbsoluteEntityCell = absoluteEntityCell;
            position = localPosition;
            rotation = localRotation;
            BiomeType = biomeType;
            Density = density;
            AllowedTypes = allowedTypes;
        }
        
        public EntitySpawnPoint(AbsoluteEntityCell absoluteEntityCell, Vector3 localPosition, Quaternion localRotation, Vector3 scale, string classId)
        {
            AbsoluteEntityCell = absoluteEntityCell;
            ClassId = classId;
            Density = 1;
            position = localPosition;
            Scale = scale;
            rotation = localRotation;
        }

        public void SetParent(EntitySpawnPoint parent)
        {
            Parent = parent;
        }

        public override string ToString() => $"[EntitySpawnPoint - {AbsoluteEntityCell}, Position: {Position}, Rotation: {Rotation}, Scale: {Scale}, ClassId: {ClassId}, BiomeType: {BiomeType}, Density: {Density}, CanSpawnCreature: {CanSpawnCreature}]";
    }
}
