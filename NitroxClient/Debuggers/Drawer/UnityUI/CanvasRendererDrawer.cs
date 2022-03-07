using System;
using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class CanvasRendererDrawer : IDrawer
{
    public Type[] ApplicableTypes { get; } = { typeof(CanvasRenderer) };

    public void Draw(object target)
    {
        switch (target)
        {
            case CanvasRenderer canvasRenderer:
                DrawCanvasRenderer(canvasRenderer);
                break;
        }
    }

    private static void DrawCanvasRenderer(CanvasRenderer canvasRenderer)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Raycast Target", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            canvasRenderer.cullTransparentMesh = NitroxGUILayout.BoolField(canvasRenderer.cullTransparentMesh);
        }
    }
}
