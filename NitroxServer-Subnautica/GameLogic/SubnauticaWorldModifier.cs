using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxServer.GameLogic;
using NitroxServer.Serialization.World;

namespace NitroxServer_Subnautica.GameLogic;

public class SubnauticaWorldModifier : IWorldModifier
{
    public void ModifyWorld(World world)
    {
        // Creating entities for the 11 RadiationLeakPoint located at (Aurora Scene) //Aurora-MainPrefab/Aurora/radiationleaks/RadiationLeaks(Clone)
        for (int i = 0; i < 11; i++)
        {
            RadiationLeakEntity leakEntity = new(new(), i, new(0));
            world.WorldEntityManager.AddGlobalRootEntity(leakEntity);
        }
    }
}
