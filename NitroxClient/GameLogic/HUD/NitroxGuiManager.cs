using System.Collections.Generic;

namespace NitroxClient.GameLogic.HUD;

public class NitroxGuiManager
{
    public List<NitroxPDATab> CustomTabs;
    public NitroxGuiManager()
    {
        CustomTabs = new();
        RegisterCustomTabs();
    }

    public void RegisterCustomTabs()
    {
        CustomTabs.Add(new PlayerListTab());
    }
}
