using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.HUD;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public class uGUI_PDA_Initialize_Patch : NitroxPatch, IDynamicPatch
{
    private readonly static MethodInfo TARGET_METHOD = Reflect.Method((uGUI_PDA t) => t.Initialize());
    private static uGUI_PDATab playerListTab;
    private static PDATab PLAYERLIST = (PDATab)8;

    public static bool Prefix(uGUI_PDA __instance)
    {
        if (__instance.initialized)
        {
            return false;
        }

        __instance.initialized = true;
        Log.Debug($"uGUI_PDA::Initialize() from : {__instance.gameObject}");
        // We need to copy the ping manager tab which is the closest to what we want
        GameObject pdaScreen = __instance.gameObject;
        uGUI_PingTab pingTab = pdaScreen.GetComponentInChildren<uGUI_PingTab>();
        GameObject pingTabObject = pingTab.gameObject;

        GameObject tabCopy = GameObject.Instantiate(pingTabObject, pdaScreen.transform.Find("Content"));
        tabCopy.name = "PlayerListTab";

        // We add a uGUI_PlayerListTab instead of a uGUI_PingTab
        uGUI_PlayerListTab newTab = tabCopy.AddComponent<uGUI_PlayerListTab>();
        newTab.MakePrefab(pingTab.prefabEntry);
        GameObject.Destroy(tabCopy.GetComponentInChildren<uGUI_PingTab>());

        playerListTab = newTab;
        uGUI_PDA.regularTabs.Add(PLAYERLIST);
        
        __instance.tabs = new Dictionary<PDATab, uGUI_PDATab>(uGUI_PDA.sPDATabComparer)
        {
            {
                PDATab.Intro,
                __instance.tabIntro
            },
            {
                PDATab.Inventory,
                __instance.tabInventory
            },
            {
                PDATab.Journal,
                __instance.tabJournal
            },
            {
                PDATab.Ping,
                __instance.tabPing
            },
            {
                PDATab.Gallery,
                __instance.tabGallery
            },
            {
                PDATab.Log,
                __instance.tabLog
            },
            {
                PDATab.Encyclopedia,
                __instance.tabEncyclopedia
            },
            {
                PDATab.TimeCapsule,
                __instance.tabTimeCapsule
            },
            {
                PLAYERLIST,
                playerListTab
            }
        };
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
