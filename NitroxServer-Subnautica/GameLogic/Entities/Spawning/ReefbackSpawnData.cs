using System.Collections.Generic;
using UnityEngine;

namespace NitroxServer_Subnautica.GameLogic.Entities.Spawning.EntityBootstrappers
{
    public static class ReefbackSpawnData
    {
        public struct ReefbackEntity
        {
            public TechType techType;
            public int minNumber;
            public int maxNumber;
            public float probability;
            public string classId;
            public Quaternion rotation;
            public Vector3 scale;
            public Vector3 position;
        }
        
        public static List<ReefbackEntity> SpawnableCreatures { get; } = new List<ReefbackEntity>();

        public static List<ReefbackEntity> SpawnablePlants { get; } = new List<ReefbackEntity>();
    }
}
