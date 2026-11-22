using Nitrox.Model.DataStructures.Unity;
using Nitrox.Server.Subnautica.Models.Attributes;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties;

[Name("SpawnRotation")]
internal sealed record RotationProperty : IPlayerProperty<NitroxQuaternion>
{
    private readonly Lock locker = new();

    public NitroxQuaternion Value
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
    } = NitroxQuaternion.Identity;
}
