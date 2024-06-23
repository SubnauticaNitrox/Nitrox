using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class CanvasRendererDrawer : IDrawer<CanvasRenderer>
{
    public void Draw(CanvasRenderer canvasRenderer)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Raycast Target", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            canvasRenderer.cullTransparentMesh = NitroxGUILayout.BoolField(canvasRenderer.cullTransparentMesh);
        }
    }
}
