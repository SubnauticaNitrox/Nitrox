using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.UnityStubs;
using NitroxModel.Logger;
using System;

namespace NitroxServer.GameLogic.Entities.Spawning
{
    public class EntitySpawnPoint
    {
        public AbsoluteEntityCell AbsoluteEntityCell { get; private set; }
        public UnityEngine.Vector3 Position { get; private set; }
        public UnityEngine.Quaternion Rotation { get; private set; }
        public UnityEngine.Vector3 Scale { get; private set; }
        public string ClassId { get; private set; }
        public string Guid { get; private set; }
        public BiomeType BiomeType { get; private set; }
        public float Density { get; private set; }
        public bool CanSpawnCreature { get; private set; }
        public List<EntitySlot.Type> AllowedTypes { get; private set; }

        public static EntitySpawnPoint From(Int3 batchId, GameObject go, Int3 cellId, int level)
        {
            // Why is this not a constructor?
            EntitySpawnPoint esp = new EntitySpawnPoint
            {
                AbsoluteEntityCell = new AbsoluteEntityCell(batchId, cellId, level),
                ClassId = go.ClassId,
                Density = 1
            };
            
            EntitySlot entitySlot = go.GetComponent<EntitySlot>();

            if (!ReferenceEquals(entitySlot, null))
            {
                esp.BiomeType = entitySlot.biomeType;
                esp.Density = entitySlot.density;
                esp.CanSpawnCreature = entitySlot.IsCreatureSlot();
                esp.AllowedTypes = entitySlot.allowedTypes;
            }

            esp.Rotation = go.GetComponent<Transform>().Rotation;

            UnityEngine.Vector3 localPosition = go.GetComponent<Transform>().Position;
            esp.Position = esp.AbsoluteEntityCell.Center + localPosition;
            esp.Scale = go.GetComponent<Transform>().Scale;

            return esp;
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            EntitySpawnPoint spawnPoint = (EntitySpawnPoint)obj;

            return spawnPoint.Position == Position && 
                   spawnPoint.ClassId == ClassId;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int classIdHash = (ClassId != null) ? ClassId.GetHashCode() : 0;

                return (269 + (Position.GetHashCode() * 23)) + classIdHash;
            }
        }

        public override string ToString() => $"[EntitySpawnPoint - {AbsoluteEntityCell}, Position: {Position}, Rotation: {Rotation}, Scale: {Scale}, ClassId: {ClassId}, Guid: {Guid}, BiomeType: {BiomeType}, Density: {Density}, CanSpawnCreature: {CanSpawnCreature}]";
    }
}
