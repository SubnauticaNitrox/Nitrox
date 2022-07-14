using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.HUD;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Add custom tabs to the 
/// </summary>
public class uGUI_PDA_Initialize_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_PDA t) => t.Initialize());

    // Added stuff is surrounded by comments
    public static bool Prefix(uGUI_PDA __instance)
    {
        if (__instance.initialized)
        {
            return false;
        }
        __instance.initialized = true;

        // Initialize all the custom tabs so that they can create their required components
        // And add their "types" to the tab list
        Resolve<NitroxPDATabManager>().CustomTabs.Values.ForEach(tab => tab.OnInitializePDA(__instance));
        Resolve<NitroxPDATabManager>().CustomTabs.Keys.ForEach(uGUI_PDA.regularTabs.Add);
        //
        
        __instance.tabs = new Dictionary<PDATab, uGUI_PDATab>(uGUI_PDA.sPDATabComparer)
        {
            { PDATab.Intro, __instance.tabIntro },
            { PDATab.Inventory, __instance.tabInventory },
            { PDATab.Journal, __instance.tabJournal },
            { PDATab.Ping, __instance.tabPing },
            { PDATab.Gallery, __instance.tabGallery },
            { PDATab.Log, __instance.tabLog },
            { PDATab.Encyclopedia, __instance.tabEncyclopedia },
            { PDATab.TimeCapsule, __instance.tabTimeCapsule }
        };

        // Also required to add the custom tabs here
        foreach (KeyValuePair<PDATab, NitroxPDATab> nitroxTab in Resolve<NitroxPDATabManager>().CustomTabs)
        {
            __instance.tabs.Add(nitroxTab.Key, nitroxTab.Value.uGUI_PDATab);
        }
        //

        foreach (KeyValuePair<PDATab, uGUI_PDATab> keyValuePair in __instance.tabs)
        {
            uGUI_PDATab value = keyValuePair.Value;
            value.Register(__instance);
            value.Close();
        }
        __instance.backButton.gameObject.SetActive(false);
        __instance.SetTabs(uGUI_PDA.regularTabs);

        return false;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
