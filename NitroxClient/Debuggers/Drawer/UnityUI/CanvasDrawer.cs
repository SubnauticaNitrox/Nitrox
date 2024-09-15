using System;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class CanvasDrawer : IDrawer<Canvas>
{
    private readonly SceneDebugger sceneDebugger;

    public CanvasDrawer(SceneDebugger sceneDebugger)
    {
        Validate.NotNull(sceneDebugger);
        this.sceneDebugger = sceneDebugger;
    }

    public void Draw(Canvas canvas)
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
                    sceneDebugger.UpdateSelectedObject(canvas.worldCamera.gameObject);
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
                        sceneDebugger.UpdateSelectedObject(canvas.worldCamera.gameObject);
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
