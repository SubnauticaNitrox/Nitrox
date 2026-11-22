using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties;

internal sealed record SubRootIdProperty : IPlayerProperty<NitroxId>
{
    public NitroxId? Value { get; set; }
}
