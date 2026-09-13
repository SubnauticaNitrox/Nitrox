using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;

internal sealed record IntroCinematicModeProperty : IConnectedPlayerProperty<IntroCinematicMode, IntroCinematicModeProperty>
{
    public IntroCinematicMode Value
    {
        get => Interlocked.CompareExchange(ref field, IntroCinematicMode.NONE, IntroCinematicMode.NONE);
        set => Interlocked.Exchange(ref field, value);
    } = IntroCinematicMode.NONE;
}
