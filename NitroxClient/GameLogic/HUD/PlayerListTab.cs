using NitroxClient.GameLogic.HUD.PdaTabs;

namespace NitroxClient.GameLogic.HUD;

public class PlayerListTab : NitroxPDATab
{
    public override string ToolbarTip()
    {
        return "Nitrox_PlayerListTabName";
    }

    public override uGUI_PDATab PdaTab()
    {
        return new uGUI_PlayerListTab();
    }
}
