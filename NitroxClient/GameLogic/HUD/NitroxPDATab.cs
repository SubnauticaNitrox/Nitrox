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
    public abstract string TabIconAssetName { get; }

    /// <summary>
    /// Base game tab from which we will take the icon as a placeholder
    /// </summary>
    public abstract PDATab FallbackTabIcon { get; }

    /// <summary>
    /// The uGUI_PDATab component that will be used in-game
    /// </summary>
    public abstract uGUI_PDATab uGUI_PDATab { get;}

    /// <summary>
    /// Should be a new int value that isn't currently used by default game (>7) nor by another custom tab
    /// </summary>
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
