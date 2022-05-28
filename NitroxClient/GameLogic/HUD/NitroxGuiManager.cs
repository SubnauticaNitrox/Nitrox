using System.Collections.Generic;

namespace NitroxClient.GameLogic.HUD;

public class NitroxGuiManager
{
    public Dictionary<PDATab, NitroxPDATab> CustomTabs;
    public NitroxGuiManager()
    {
        CustomTabs = new();
        RegisterCustomTabs();
    }

    public void RegisterCustomTabs()
    {
        RegisterTab(new PlayerListTab());
    }

    private void RegisterTab(NitroxPDATab nitroxTab)
    {
        CustomTabs.Add(nitroxTab.PDATabId(), nitroxTab);
    }
}
