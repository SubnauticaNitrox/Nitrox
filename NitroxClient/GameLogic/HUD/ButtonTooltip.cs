using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.GameLogic.HUD;

public class ButtonTooltip : MonoBehaviour, ITooltip
{
    public string TooltipText { get; set; }

    public void GetTooltip(out string tooltipText, List<TooltipIcon> tooltipIcons)
    {
        tooltipText = TooltipText;
    }
}
