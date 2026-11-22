using Nitrox.Model.DataStructures.Unity;
using Nitrox.Server.Subnautica.Models.Attributes;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties;

[Name("SpawnPosition")]
internal sealed record PositionProperty : IPlayerProperty<NitroxVector3>
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
