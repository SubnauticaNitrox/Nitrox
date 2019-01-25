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

        public static List<Vector3> LocalCreatureSpawnPoints { get; } = new List<Vector3>() // Think i deprecated this thing
        {
            new Vector3(-22.9f, 17.0f, 0.0f),
            new Vector3(5.1f, 17.9f, 22.1f),
            new Vector3(5.1f, 17.9f, -11.6f),
            new Vector3(-5.1f, 17.9f, 5.6f),
            new Vector3(23.6f, 17.9f, 5.6f),
            new Vector3(-16.4f, 17.9f, 25.9f),
            new Vector3(-8.7f, 17.9f, -30.3f),
            new Vector3(15.4f, 17.9f, -30.3f),
            new Vector3(20.9f, 17.9f, -13.9f),
            new Vector3(-17.3f, 17.9f, 22.1f)
        };
    }
}
