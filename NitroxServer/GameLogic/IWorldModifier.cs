using NitroxServer.Serialization.World;

namespace NitroxServer.GameLogic;

/// <summary>
/// Holds a set of instructions to be ran when a world is created. There should be one Subnautica and one for BZ.
/// </summary>
public interface IWorldModifier
{
    public void ModifyWorld(World world);
}
