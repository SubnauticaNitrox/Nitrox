using System.Collections.Generic;

namespace NitroxModel.DataStructures.GameLogic.Entities;

public interface IUwePrefabFactory
{
    public abstract List<UwePrefab> GetPossiblePrefabs(string biomeType);
}
