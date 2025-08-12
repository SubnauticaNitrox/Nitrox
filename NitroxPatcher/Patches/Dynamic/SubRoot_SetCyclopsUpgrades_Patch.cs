using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// The way Subnautica handles modules in Cyclops wrecks is pretty weird. if any module is added/removed (and when spawning
/// the entity during joining), they are all instantly disabled, which deletes creature decoys in any slot except
/// the first one. We want to keep the creature decoys around if the module is inserted, so we allow use of that
/// module even when the Cyclops has been destroyed.
/// </summary>
public sealed partial class SubRoot_SetCyclopsUpgrades_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubRoot t) => t.SetCyclopsUpgrades());

    public static void Postfix(SubRoot __instance)
    {
        if (__instance.upgradeConsole == null)
        {
            return;
        }

        __instance.decoyTubeSizeIncreaseUpgrade = false;

        Equipment modules = __instance.upgradeConsole.modules;
        foreach (string slot in SubRoot.slotNames)
        {
            TechType techTypeInSlot = modules.GetTechTypeInSlot(slot);
            if (techTypeInSlot == TechType.CyclopsDecoyModule)
            {
                __instance.decoyTubeSizeIncreaseUpgrade = true;
                break;
            }
        }
    }
}
