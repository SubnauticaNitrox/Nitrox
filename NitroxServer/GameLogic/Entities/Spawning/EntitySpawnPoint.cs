using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using UnityEngine;

namespace NitroxServer.GameLogic.Entities.Spawning
{
    public class EntitySpawnPoint
    {
        public readonly List<EntitySpawnPoint> Children = new List<EntitySpawnPoint>();

        public Vector3 LocalPosition;

        public Quaternion LocalRotation;
        public AbsoluteEntityCell AbsoluteEntityCell { get; }
        public Vector3 Position => Parent != null ? Parent.Position + LocalPosition : LocalPosition;
        public Quaternion Rotation => Parent != null ? Parent.Rotation * LocalRotation : LocalRotation;
        public Vector3 Scale { get; }
        public string ClassId { get; }
        public string BiomeType { get; }
        public float Density { get; }
        public bool CanSpawnCreature { get; private set; }
        public List<string> AllowedTypes { get; }

        public EntitySpawnPoint Parent { get; set; }

        public EntitySpawnPoint(AbsoluteEntityCell absoluteEntityCell, Vector3 localPosition, Quaternion localRotation, List<string> allowedTypes, float density, string biomeType)
        {
            AbsoluteEntityCell = absoluteEntityCell;
            LocalPosition = localPosition;
            LocalRotation = localRotation;
            BiomeType = biomeType;
            Density = density;
            AllowedTypes = allowedTypes;
        }

        public EntitySpawnPoint(AbsoluteEntityCell absoluteEntityCell, Vector3 localPosition, Quaternion localRotation, Vector3 scale, string classId)
        {
            AbsoluteEntityCell = absoluteEntityCell;
            ClassId = classId;
            Density = 1;
            LocalPosition = localPosition;
            Scale = scale;
            LocalRotation = localRotation;
        }

        public override string ToString() => $"[EntitySpawnPoint - {AbsoluteEntityCell}, Position: {Position}, Rotation: {Rotation}, Scale: {Scale}, ClassId: {ClassId}, BiomeType: {BiomeType}, Density: {Density}, CanSpawnCreature: {CanSpawnCreature}]";
    }
}
