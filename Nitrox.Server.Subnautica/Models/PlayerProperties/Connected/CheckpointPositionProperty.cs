using Nitrox.Model.DataStructures.Unity;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;

internal sealed record CheckpointPositionProperty : IConnectedPlayerProperty<NitroxVector3, CheckpointPositionProperty>
{
    private readonly Lock locker = new();

    public NitroxVector3 Value
    {
        get
        {
            lock (locker)
            {
                return field;
            }
        }
        set
        {
            lock (locker)
            {
                field = value;
            }
        }
    } = NitroxVector3.Zero;
}
