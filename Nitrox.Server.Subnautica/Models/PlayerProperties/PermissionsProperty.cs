using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties;

// TODO: Change this to IPlayerProperty so it's persisted. But only after security of player login is improved by https://github.com/SubnauticaNitrox/Nitrox/issues/1996
internal sealed class PermissionsProperty(IOptions<SubnauticaServerOptions> options) : IConnectedPlayerProperty<Perms, PermissionsProperty>
{
    private readonly IOptions<SubnauticaServerOptions> options = options;

    public PermissionsProperty() : this(new OptionsWrapper<SubnauticaServerOptions>(new SubnauticaServerOptions()))
    {
    }

    public Perms Value
    {
        get => Interlocked.CompareExchange(ref field, Perms.DEFAULT, Perms.DEFAULT);
        set => Interlocked.Exchange(ref field, value);
    } = options.Value.DefaultPlayerPerm;

    public Task ResetAsync()
    {
        // We need to reset permissions on join, otherwise players can impersonate an admin easily.
        Value = options.Value.DefaultPlayerPerm;
        return Task.CompletedTask;
    }
}
