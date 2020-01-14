using System.Collections.Generic;

namespace NitroxModel.DataStructures.GameLogic.Entities
{
    public abstract class UwePrefabFactory
    {
        public abstract List<UwePrefab> GetPossiblePrefabs(string biomeType);
    }
}
