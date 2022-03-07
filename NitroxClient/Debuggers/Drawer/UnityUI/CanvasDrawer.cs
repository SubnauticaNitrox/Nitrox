using System;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class CanvasDrawer : IDrawer
{
    public Type[] ApplicableTypes { get; } = { typeof(Canvas) };

    public void Draw(object target)
    {
        switch (target)
        {
            case Canvas canvas:
                DrawCanvas(canvas);
                break;
        }
    }

    private static void DrawCanvas(Canvas canvas)
    {
        RenderMode renderMode = canvas.renderMode;

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Raycast Target", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            renderMode = NitroxGUILayout.EnumPopup(renderMode);
        }

        if (renderMode == RenderMode.WorldSpace)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Event Camera", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                NitroxGUILayout.Separator();
                if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
                {
                    NitroxServiceLocator.Cache<SceneDebugger>.Value.UpdateSelectedObject(canvas.worldCamera.gameObject);
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Sorting layer", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                NitroxGUILayout.Separator();
                canvas.sortingLayerID = NitroxGUILayout.IntField(canvas.sortingLayerID);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Order in Layer", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                NitroxGUILayout.Separator();
                canvas.sortingOrder = NitroxGUILayout.IntField(canvas.sortingOrder);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Additional Shader Channels", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                NitroxGUILayout.Separator();
                canvas.additionalShaderChannels = NitroxGUILayout.EnumPopup(canvas.additionalShaderChannels);
            }
        }
        else
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Pixel Perfect", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                NitroxGUILayout.Separator();
                canvas.pixelPerfect = NitroxGUILayout.BoolField(canvas.pixelPerfect);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Sort Order", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                NitroxGUILayout.Separator();
                canvas.sortingOrder = NitroxGUILayout.IntField(canvas.sortingOrder);
            }

            if (renderMode == RenderMode.ScreenSpaceOverlay)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Target Display", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                    NitroxGUILayout.Separator();
                    int newTargetDisplay = NitroxGUILayout.IntField(canvas.targetDisplay);
                    canvas.targetDisplay = Math.Max(0, Math.Min(8, newTargetDisplay));
                }
            }
            else
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Render Camera", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                    NitroxGUILayout.Separator();
                    if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
                    {
                        NitroxServiceLocator.Cache<SceneDebugger>.Value.UpdateSelectedObject(canvas.worldCamera.gameObject);
                    }
                }

                if (canvas.worldCamera)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Plane Distance", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                        NitroxGUILayout.Separator();
                        canvas.planeDistance = NitroxGUILayout.FloatField(canvas.planeDistance);
                    }
                }
            }
        }
    }
}
