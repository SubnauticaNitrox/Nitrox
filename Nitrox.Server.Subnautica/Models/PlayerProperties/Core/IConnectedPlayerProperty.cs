namespace Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

/// <summary>
///     Defines a per-player property that will reset when the respective player (re)connects.
/// </summary>
/// <remarks>
///     This property won't be persisted.
/// </remarks>
/// <typeparam name="T">The <see cref="Type" /> of the property.</typeparam>
/// <typeparam name="TProp"></typeparam>
internal interface IConnectedPlayerProperty<T, TProp> : IPlayerProperty<T>, IConnectedPlayerProperty
    where TProp : IPlayerProperty<T>, new()
{
    /// <inheritdoc cref="IConnectedPlayerProperty.ResetAsync" />
    new Task ResetAsync()
    {
        Value = new TProp().Value;
        return Task.CompletedTask;
    }

    Task IConnectedPlayerProperty.ResetAsync() => ResetAsync();
}

internal interface IConnectedPlayerProperty : IPlayerProperty
{
    /// <summary>
    ///     Resets the property to the default value. This is called when the player connects to this server.
    /// </summary>
    Task ResetAsync();
}
