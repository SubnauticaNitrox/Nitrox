using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;

internal sealed record CheckpointSubRootIdProperty : IConnectedPlayerProperty<NitroxId?, CheckpointSubRootIdProperty>
{
    public NitroxId? Value { get; set; }
}
