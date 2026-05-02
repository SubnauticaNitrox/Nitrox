using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;

public interface IUwePrefabFactory
{
    Task<List<UwePrefab>> TryGetPossiblePrefabsAsync(string? biome);
}
