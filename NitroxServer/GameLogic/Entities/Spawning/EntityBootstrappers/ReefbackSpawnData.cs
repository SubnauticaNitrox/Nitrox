using System.Collections.Generic;
using UnityEngine;

namespace NitroxServer.GameLogic.Entities.Spawning.EntityBootstrappers
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
        }
        
        public static List<ReefbackEntity> SpawnableCreatures { get; } = new List<ReefbackEntity>()
        {
            new ReefbackEntity() { techType = TechType.Peeper, probability = 1, minNumber = 1, maxNumber = 2 },
            new ReefbackEntity() { techType = TechType.Boomerang, probability = 1, minNumber = 1, maxNumber = 2 },
            new ReefbackEntity() { techType = TechType.Hoverfish, probability = 1, minNumber = 1, maxNumber = 2 },
            new ReefbackEntity() { techType = TechType.Bladderfish, probability = 1, minNumber = 1, maxNumber = 2 },
            new ReefbackEntity() { techType = TechType.Eyeye, probability = 1, minNumber = 1, maxNumber = 1 },
            new ReefbackEntity() { techType = TechType.HoleFish, probability = 1, minNumber = 1, maxNumber = 1 },
            new ReefbackEntity() { techType = TechType.Hoopfish, probability = 1, minNumber = 1, maxNumber = 1 },
            new ReefbackEntity() { techType = TechType.Reginald, probability = 1, minNumber = 1, maxNumber = 2 },
            new ReefbackEntity() { techType = TechType.Spadefish, probability = 1, minNumber = 1, maxNumber = 2 },
            new ReefbackEntity() { techType = TechType.Biter, probability = 0.5f, minNumber = 1, maxNumber = 3 },
            new ReefbackEntity() { techType = TechType.HoopfishSchool, probability = 1, minNumber = 3, maxNumber = 3 }
        };

        public static List<Vector3> LocalCreatureSpawnPoints { get; } = new List<Vector3>()
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
