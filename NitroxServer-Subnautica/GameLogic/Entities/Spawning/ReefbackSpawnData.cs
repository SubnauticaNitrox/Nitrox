using System.Collections.Generic;
using UnityEngine;

namespace NitroxServer_Subnautica.GameLogic.Entities.Spawning.EntityBootstrappers
{
    public static class ReefbackSpawnData
    {
        public struct ReefbackCreature
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
        
        public static List<ReefbackCreature> SpawnableCreatures { get; } = new List<ReefbackCreature>();

        public static List<ReefbackPlant> SpawnablePlants { get; set; } = new List<ReefbackPlant>();

        public struct ReefbackPlant
        {
            public string classId;
            public TechType techType;
            public float probability;
            public Quaternion rotation;
            public Vector3 scale;
            public Vector3 position;
        }
    }
}
