using System;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class ToggleGroupDrawer : IDrawer
{
    public Type[] ApplicableTypes { get; } = { typeof(ToggleGroup) };

    public void Draw(object target)
    {
        switch (target)
        {
            case ToggleGroup toggleGroup:
                DrawToggleGroup(toggleGroup);
                break;
        }
    }

    private static void DrawToggleGroup(ToggleGroup toggleGroup)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Allow Switch Off", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            toggleGroup.allowSwitchOff = NitroxGUILayout.BoolField(toggleGroup.allowSwitchOff);
        }
    }
}
