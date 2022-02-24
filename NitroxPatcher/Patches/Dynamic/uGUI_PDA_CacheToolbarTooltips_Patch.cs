using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class uGUI_PDA_CacheToolbarTooltips_Patch : NitroxPatch, IDynamicPatch
{

    private readonly static MethodInfo TARGET_METHOD = Reflect.Method((uGUI_PDA t) => t.CacheToolbarTooltips());

    public static bool Prefix(uGUI_PDA __instance)
    {
        __instance.toolbarTooltips.Clear();
        for (int i = 0; i < __instance.currentTabs.Count; i++)
        {
            PDATab pdatab = __instance.currentTabs[i];
            __instance.toolbarTooltips.Add(TooltipFactory.Label(string.Format("Tab{0}", pdatab.ToString())));
        }
        // Modify the last tooltip which is the one of the newly created tab in uGUI_PDA_Initialize_Patch
        __instance.toolbarTooltips[__instance.toolbarTooltips.Count - 1] = TooltipFactory.Label(string.Format("Nitrox_PlayerListTabName"));
        return false;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
