using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxServer.GameLogic;
using NitroxServer.Serialization.World;

namespace NitroxServer_Subnautica.GameLogic;

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
