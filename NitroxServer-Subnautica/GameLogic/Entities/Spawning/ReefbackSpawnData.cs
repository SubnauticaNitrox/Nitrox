using System.Collections.Generic;
using UnityEngine;

namespace NitroxServer_Subnautica.GameLogic.Entities.Spawning.EntityBootstrappers
{
    public static class ReefbackSpawnData
    {
        public struct ReefbackEntity
        {
            public TechType TechType;
            public int MinNumber;
            public int MaxNumber;
            public float Probability;
            public string ClassId;
            public Quaternion Rotation;
            public Vector3 Scale;
            public Vector3 Position;
        }
        
        public static List<ReefbackEntity> SpawnableCreatures { get; } = new List<ReefbackEntity>();

        public static List<ReefbackEntity> SpawnablePlants { get; } = new List<ReefbackEntity>();
    }
}
