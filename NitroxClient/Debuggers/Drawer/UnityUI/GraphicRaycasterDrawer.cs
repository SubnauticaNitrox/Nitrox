using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class GraphicRaycasterDrawer : IDrawer<GraphicRaycaster>
{
    public void Draw(GraphicRaycaster graphicRaycaster)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Ignore Reversed Graphics", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            graphicRaycaster.ignoreReversedGraphics = NitroxGUILayout.BoolField(graphicRaycaster.ignoreReversedGraphics);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Blocking Objects", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            graphicRaycaster.blockingObjects = NitroxGUILayout.EnumPopup(graphicRaycaster.blockingObjects);
        }
    }
}
