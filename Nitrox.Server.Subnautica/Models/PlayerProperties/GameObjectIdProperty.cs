using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.Attributes;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties;

[Name("NitroxId")]
internal sealed class GameObjectIdProperty : IPlayerProperty<NitroxId>
{
    public NitroxId Value
    {
        get { return Interlocked.CompareExchange(ref field, null, null); }
        set => Interlocked.Exchange(ref field, value);
    } = new();
}
