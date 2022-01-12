using System;
using NitroxClient.Debuggers.Drawer.Unity;
using NitroxModel.Core;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class ToggleDrawer : IDrawer
{
    public Type[] ApplicableTypes { get; } = { typeof(Toggle) };

    public void Draw(object target)
    {
        switch (target)
        {
            case Toggle toggle:
                DrawToggle(toggle);
                break;
        }
    }

    private static void DrawToggle(Toggle toggle)
    {
        SelectableDrawer.DrawSelectable(toggle);
        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Is On", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            toggle.isOn = NitroxGUILayout.BoolField(toggle.isOn);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Toggle Transition", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            toggle.toggleTransition = NitroxGUILayout.EnumPopup(toggle.toggleTransition);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Graphic", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                NitroxServiceLocator.Cache<SceneDebugger>.Value.UpdateSelectedObject(toggle.graphic.gameObject);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Group", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                NitroxServiceLocator.Cache<SceneDebugger>.Value.UpdateSelectedObject(toggle.group.gameObject);
            }
        }

        UnityEventDrawer.DrawUnityEventBool(toggle.onValueChanged, "OnClick()");
    }
}
