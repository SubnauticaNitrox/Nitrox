using UnityEngine;

namespace NitroxClient.GameLogic.HUD.Components;

/// <summary>
/// Shows the ToolipText when hovering the GameObject containg this component
/// </summary>
public class ButtonTooltip : MonoBehaviour, ITooltip
{
    public string TooltipText { get; set; }
    public bool showTooltipOnDrag => false;

    public void GetTooltip(TooltipData tooltip)
    {
        tooltip.prefix.Append(TooltipText);
    }
}
