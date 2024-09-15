using NitroxClient.Debuggers.Drawer.Unity;
using NitroxModel.Helper;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class ToggleDrawer : IDrawer<Toggle>
{
    private readonly SceneDebugger sceneDebugger;
    private readonly SelectableDrawer selectableDrawer;
    private readonly UnityEventDrawer unityEventDrawer;

    public ToggleDrawer(SceneDebugger sceneDebugger, SelectableDrawer selectableDrawer, UnityEventDrawer unityEventDrawer)
    {
        Validate.NotNull(sceneDebugger);
        Validate.NotNull(selectableDrawer);
        Validate.NotNull(unityEventDrawer);

        this.sceneDebugger = sceneDebugger;
        this.selectableDrawer = selectableDrawer;
        this.unityEventDrawer = unityEventDrawer;
    }

    public void Draw(Toggle toggle)
    {
        selectableDrawer.Draw(toggle);
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
                sceneDebugger.UpdateSelectedObject(toggle.graphic.gameObject);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Group", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                sceneDebugger.UpdateSelectedObject(toggle.group.gameObject);
            }
        }

        unityEventDrawer.Draw(toggle.onValueChanged, new UnityEventDrawer.DrawOptions("OnClick()"));
    }
}
