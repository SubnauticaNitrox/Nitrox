using System.Reflection;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Persistent;

public sealed partial class CyclopsSonarCreatureDetector_CheckForCreaturesInRange_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsSonarCreatureDetector t) => t.CheckForCreaturesInRange());

    public const CyclopsSonarDisplay.EntityType PLAYER_TYPE = (CyclopsSonarDisplay.EntityType)2;

    public static void Postfix(CyclopsSonarCreatureDetector __instance)
    {
        __instance.ChekItemsOnHashSet(Resolve<PlayerManager>().GetAllPlayerObjects(), PLAYER_TYPE);
    }
}
