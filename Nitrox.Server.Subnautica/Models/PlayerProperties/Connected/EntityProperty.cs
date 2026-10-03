using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;

/// <summary>
///     Stores the player entity, not persisted and session only.
/// </summary>
internal sealed class EntityProperty : IConnectedPlayerProperty<PlayerEntity, EntityProperty>
{
    public PlayerEntity Value
    {
        get => Interlocked.CompareExchange(ref field, null, null);
        set => Interlocked.Exchange(ref field, value);
    }
}
