using NitroxClient.GameLogic.HUD.PdaTabs;
using UnityEngine;

namespace NitroxClient.GameLogic.HUD;

public class PlayerListTab : NitroxPDATab
{
    private uGUI_PDATab tab;

    public override string ToolbarTip()
    {
        return "Nitrox_PlayerListTabName";
    }

    public override uGUI_PDATab uGUI_PDATab()
    {
        return tab;
    }

    public override PDATab PDATabId()
    {
        return (PDATab)8;
    }

    public override void OnInitializePDA(uGUI_PDA uGUI_PDA)
    {
        // We need to copy the ping manager tab which is the closest to what we want
        GameObject pdaScreen = uGUI_PDA.gameObject;
        uGUI_PingTab pingTab = pdaScreen.GetComponentInChildren<uGUI_PingTab>();
        GameObject pingTabObject = pingTab.gameObject;

        GameObject tabCopy = GameObject.Instantiate(pingTabObject, pdaScreen.transform.Find("Content"));
        tabCopy.name = "PlayerListTab";

        // We add a uGUI_PlayerListTab instead of a uGUI_PingTab
        uGUI_PlayerListTab newTab = tabCopy.AddComponent<uGUI_PlayerListTab>();
        newTab.MakePrefab(pingTab.prefabEntry);
        GameObject.Destroy(tabCopy.GetComponentInChildren<uGUI_PingTab>());

        tab = newTab;
    }

    public override bool KeepPingsVisible()
    {
        return true;
    }
}
