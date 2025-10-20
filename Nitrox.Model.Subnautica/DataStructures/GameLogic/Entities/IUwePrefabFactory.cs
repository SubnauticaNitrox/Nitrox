using System.Collections.Generic;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;

public interface IUwePrefabFactory
{
    public bool TryGetPossiblePrefabs(string biomeType, out List<UwePrefab> prefabs);
}
