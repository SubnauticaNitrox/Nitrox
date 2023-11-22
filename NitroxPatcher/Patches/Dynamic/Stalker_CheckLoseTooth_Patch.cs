using System.Reflection;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Stalker_CheckLoseTooth_Patch : NitroxPatch, IDynamicPatch
{
    public override MethodInfo targetMethod { get; } = Reflect.Method((Stalker t) => t.CheckLoseTooth(default(GameObject)));

    //GetComponent<HardnessMixin> was returning null for everything instead of a HardnessMixin with a hardness value. Since this component
    //isn't used for anything else than the stalker teeth drop, we hard-code the values and bingo.
    public static bool Prefix(Stalker __instance, GameObject target)
    {
        float dropProbability = 0f;
        TechType techType = CraftData.GetTechType(target);

        if (techType == TechType.ScrapMetal)
        {
            dropProbability = 0.15f; // 15% probability
        }

        if (UnityEngine.Random.value < dropProbability)
        {
            __instance.LoseTooth();
        }
        return false;
    }
}
