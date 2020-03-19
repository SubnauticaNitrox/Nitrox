using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;

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
        }
        
        public static List<ReefbackEntity> SpawnableCreatures { get; } = new List<ReefbackEntity>()
        {
            new ReefbackEntity() { techType = TechType.Peeper, probability = 1, minNumber = 1, maxNumber = 2, classId = "3fcd548b-781f-46ba-b076-7412608deeef" },
            new ReefbackEntity() { techType = TechType.Boomerang, probability = 1, minNumber = 1, maxNumber = 2, classId = "fa4cfe65-4eaf-4d51-ba0d-e8cc9632fd47" },
            new ReefbackEntity() { techType = TechType.Hoverfish, probability = 1, minNumber = 1, maxNumber = 2, classId = "0a993944-87d3-441e-b21d-6c314f723cc7" },
            new ReefbackEntity() { techType = TechType.Bladderfish, probability = 1, minNumber = 1, maxNumber = 2, classId = "bf9ccd04-60af-4144-aaa1-4ac184c686c2" },
            new ReefbackEntity() { techType = TechType.Eyeye, probability = 1, minNumber = 1, maxNumber = 1, classId = "79c1aef0-e505-469c-ab36-c22c76aeae44" },
            new ReefbackEntity() { techType = TechType.HoleFish, probability = 1, minNumber = 1, maxNumber = 1, classId = "495befa0-0e6b-400d-9734-227e5a732f75" },
            new ReefbackEntity() { techType = TechType.Hoopfish, probability = 1, minNumber = 1, maxNumber = 1, classId = "284ceeb6-b437-4aca-a8bd-d54f336cbef8" },
            new ReefbackEntity() { techType = TechType.Reginald, probability = 1, minNumber = 1, maxNumber = 2, classId = "8e82dc63-5991-4c63-a12c-2aa39373a7cf" },
            new ReefbackEntity() { techType = TechType.Spadefish, probability = 1, minNumber = 1, maxNumber = 2, classId = "d040bec1-0368-4f7c-aed6-93b5e1852d45" },
            new ReefbackEntity() { techType = TechType.Biter, probability = 0.5f, minNumber = 1, maxNumber = 3, classId = "4064a71a-c464-4db2-942a-56391fe69951" },
            new ReefbackEntity() { techType = TechType.HoopfishSchool, probability = 1, minNumber = 3, maxNumber = 3, classId = "08cb3290-504b-4191-97ee-6af1588af5c0" }
        };

        public static List<NitroxVector3> LocalCreatureSpawnPoints { get; } = new List<NitroxVector3>()
        {
            new NitroxVector3(-22.9f, 17.0f, 0.0f),
            new NitroxVector3(5.1f, 17.9f, 22.1f),
            new NitroxVector3(5.1f, 17.9f, -11.6f),
            new NitroxVector3(-5.1f, 17.9f, 5.6f),
            new NitroxVector3(23.6f, 17.9f, 5.6f),
            new NitroxVector3(-16.4f, 17.9f, 25.9f),
            new NitroxVector3(-8.7f, 17.9f, -30.3f),
            new NitroxVector3(15.4f, 17.9f, -30.3f),
            new NitroxVector3(20.9f, 17.9f, -13.9f),
            new NitroxVector3(-17.3f, 17.9f, 22.1f)
        };
    }
}
