using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities;

/// <summary>
/// Receives committed changes to the server's world-entity indexes.
/// Implementations must not mutate the observed entity or call back into
/// <see cref="WorldEntityManager"/> synchronously.
/// </summary>
internal interface IWorldEntityLifecycleObserver
{
    void EntityTracked(WorldEntity entity);

    void EntityMoved(WorldEntity entity);

    void EntityUntracked(WorldEntity entity);
}
