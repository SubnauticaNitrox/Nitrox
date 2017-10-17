using NitroxServer.UnityStubs;
using System;

namespace NitroxServer.GameLogic.Spawning
{
    public class SpawnedEntity
    {
        private Vector3 Position { get; }
        private TechType TechType { get; }
        private String Guid { get; }
        private bool IsCreature { get; }

        public SpawnedEntity(Vector3 position, TechType techType, String guid, bool isCreature)
        {
            this.Position = position;
            this.TechType = techType;
            this.Guid = guid;
            this.IsCreature = isCreature;
        }

    }
}
