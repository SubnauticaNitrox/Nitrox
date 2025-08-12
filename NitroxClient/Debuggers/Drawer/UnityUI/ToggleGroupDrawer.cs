using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class ToggleGroupDrawer : IDrawer<ToggleGroup>
{
    public void Draw(ToggleGroup toggleGroup)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Allow Switch Off", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            toggleGroup.allowSwitchOff = NitroxGUILayout.BoolField(toggleGroup.allowSwitchOff);
        }
    }
}
