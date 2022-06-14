namespace NitroxClient.GameLogic.HUD;

public abstract class NitroxPDATab
{
    /// <summary>
    /// Text showing up when hovering the tab icon
    /// </summary>
    public abstract string ToolbarTip { get; }

    /// <summary>
    /// Asset name for the tab's icon sprite
    /// </summary>
    public abstract string TabIconName { get; }

    /// <summary>
    /// The uGUI_PDATab component that will be used in-game
    /// </summary>
    public abstract uGUI_PDATab uGUI_PDATab { get;}

    public abstract PDATab PDATabId { get; }

    /// <summary>
    /// Create uGUI_PDATab component thanks to the now existing uGUI_PDA component
    /// </summary>
    public abstract void OnInitializePDA(uGUI_PDA uGUI_PDA);

    /// <summary>
    /// Whether or not to render the pings images over the PDA when this PDA tab is open
    /// </summary>
    public abstract bool KeepPingsVisible { get; }
}
