using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;

internal sealed record DrivingVehicleProperty : IConnectedPlayerProperty<NitroxId, DrivingVehicleProperty>
{
    public NitroxId? Value
    {
        get => Interlocked.CompareExchange(ref field, null, null);
        set => Interlocked.Exchange(ref field, value);
    } = null;
}
