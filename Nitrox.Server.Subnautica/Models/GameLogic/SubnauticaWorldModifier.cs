using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Serialization.World;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

public class SubnauticaWorldModifier : IWorldModifier
{
    // This constant is defined by Subnautica and should never be modified
    private const int TOTAL_LEAKS = 11;

    public void ModifyWorld(World world)
    {
        // Creating entities for the 11 RadiationLeakPoint located at (Aurora Scene) //Aurora-MainPrefab/Aurora/radiationleaks/RadiationLeaks(Clone)
        for (int i = 0; i < TOTAL_LEAKS; i++)
        {
            RadiationLeakEntity leakEntity = new(new(), i, new(0));
            world.WorldEntityManager.AddOrUpdateGlobalRootEntity(leakEntity);
        }
    }
}
