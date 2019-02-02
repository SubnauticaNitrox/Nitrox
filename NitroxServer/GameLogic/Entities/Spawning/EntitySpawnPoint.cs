﻿using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

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
        
        public static EntitySpawnPoint From(AbsoluteEntityCell absoluteEntityCell, Vector3 localPosition, Quaternion localRotation, Vector3 scale, string classId, IEntitySlot entitySlot)
        {
            EntitySpawnPoint spawnPoint = From(absoluteEntityCell, localPosition, localRotation, scale, classId);

            spawnPoint.BiomeType = entitySlot.GetBiomeType();
            spawnPoint.Density = entitySlot.GetDensity();
            spawnPoint.CanSpawnCreature = entitySlot.IsCreatureSlot();
            spawnPoint.AllowedTypes = SlotsHelper.GetEntitySlotTypes(entitySlot);

            return spawnPoint;
        }

        public static List<EntitySpawnPoint> From(AbsoluteEntityCell absoluteEntityCell, EntitySlotsPlaceholder placeholder)
        {
            List<EntitySpawnPoint> esp = new List<EntitySpawnPoint>();
            foreach (EntitySlotData entitySlot in placeholder.slotsData)
            {
                esp.Add(new EntitySpawnPoint
                {
                    AbsoluteEntityCell = absoluteEntityCell,
                    BiomeType = entitySlot.biomeType,
                    Density = entitySlot.density,
                    Position = absoluteEntityCell.Center + entitySlot.localPosition,
                    Rotation = entitySlot.localRotation,
                    AllowedTypes = SlotsHelper.GetEntitySlotTypes(entitySlot),
                });
            }

            return esp;
        }

        public static EntitySpawnPoint From(AbsoluteEntityCell absoluteEntityCell, Vector3 localPosition, Quaternion localRotation, Vector3 scale, string classId)
        {
            EntitySpawnPoint esp = new EntitySpawnPoint
            {
                AbsoluteEntityCell = absoluteEntityCell,
                ClassId = classId,
                Density = 1
            };

            esp.Position = esp.AbsoluteEntityCell.Center + localPosition;
            esp.Scale = scale;
            esp.Rotation = localRotation;

            return esp;
        }

        public override string ToString() => $"[EntitySpawnPoint - {AbsoluteEntityCell}, Position: {Position}, Rotation: {Rotation}, Scale: {Scale}, ClassId: {ClassId}, Guid: {Guid}, BiomeType: {BiomeType}, Density: {Density}, CanSpawnCreature: {CanSpawnCreature}]";
    }
}
