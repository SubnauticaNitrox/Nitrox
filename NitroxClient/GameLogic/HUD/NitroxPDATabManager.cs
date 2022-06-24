using System.Collections.Generic;

namespace NitroxClient.GameLogic.HUD;

public class NitroxPDATabManager
{
    public Dictionary<PDATab, NitroxPDATab> CustomTabs = new();

    public NitroxPDATabManager()
    {
        RegisterTab(new PlayerListTab());
    }

    private void RegisterTab(NitroxPDATab nitroxTab)
    {
        CustomTabs.Add(nitroxTab.PDATabId, nitroxTab);
    }
}
