using NitroxClient.GameLogic.HUD.PdaTabs;
using UnityEngine;

namespace NitroxClient.GameLogic.HUD;

public class PlayerListTab : NitroxPDATab
{
    private uGUI_PDATab tab;

    public override string ToolbarTip => "Nitrox_PlayerListTabName";

    public override string TabIconAssetName => "player_list_tab@3x";
    
    public override PDATab FallbackTabIcon => PDATab.Inventory;

    public override uGUI_PDATab uGUI_PDATab => tab;

    public override PDATab PDATabId => (PDATab)8;

    public override void OnInitializePDA(uGUI_PDA uGUI_PDA)
    {
        // We need to copy the ping manager tab which is the closest to what we want
        GameObject pdaScreen = uGUI_PDA.gameObject;
        uGUI_PingTab pingTab = pdaScreen.GetComponentInChildren<uGUI_PingTab>();
        GameObject pingTabObject = pingTab.gameObject;

        GameObject tabCopy = GameObject.Instantiate(pingTabObject, pdaScreen.transform.Find("Content"));
        tabCopy.name = "PlayerListTab";

        // Set the tab inactive to suppress the uGUI_PlayerListTab awake().  We first need to set the 
        // prefab, which is used inside the awake method and the base class awake method.
        tabCopy.SetActive(false);
        uGUI_PlayerListTab newTab = tabCopy.AddComponent<uGUI_PlayerListTab>();
        newTab.MakePrefab(pingTab.prefabEntry);
        GameObject.DestroyImmediate(tabCopy.GetComponent<uGUI_PingTab>());
        tabCopy.SetActive(true);

        tab = newTab;
    }

    public override bool KeepPingsVisible => true;
}
