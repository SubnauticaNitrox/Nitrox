using System.Collections.Generic;

namespace Nitrox.Model.DataStructures.GameLogic.Entities
{
    public abstract class UwePrefabFactory
    {
        public abstract List<UwePrefab> GetPossiblePrefabs(string biomeType);
    }
}
