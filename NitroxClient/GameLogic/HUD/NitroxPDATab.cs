namespace NitroxClient.GameLogic.HUD;

public abstract class NitroxPDATab
{
    /// <summary>
    /// Text showing up when hovering the tab icon
    /// </summary>
    public abstract string ToolbarTip();

    /// <summary>
    /// The uGUI_PDATab component that will be used in-game
    /// </summary>
    public abstract uGUI_PDATab PdaTab();
}
