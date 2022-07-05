using System;
using NitroxClient.Debuggers.Drawer.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class CanvasScalerDrawer : IDrawer
{
    public Type[] ApplicableTypes { get; } = { typeof(CanvasScaler) };

    public void Draw(object target)
    {
        switch (target)
        {
            case CanvasScaler canvasScaler:
                DrawCanvasScaler(canvasScaler);
                break;
        }
    }

    private static void DrawCanvasScaler(CanvasScaler canvasScaler)
    {
        if (canvasScaler.GetComponent<Canvas>().renderMode == RenderMode.WorldSpace)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Dynamic Pixels Per Unit", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                NitroxGUILayout.Separator();
                canvasScaler.dynamicPixelsPerUnit = NitroxGUILayout.FloatField(canvasScaler.dynamicPixelsPerUnit);
            }
        }
        else
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("UI Scale Mode", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                NitroxGUILayout.Separator();
                canvasScaler.uiScaleMode = NitroxGUILayout.EnumPopup(canvasScaler.uiScaleMode);
            }

            if (canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPixelSize)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Scale Factor", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                    NitroxGUILayout.Separator();
                    canvasScaler.scaleFactor = NitroxGUILayout.FloatField(canvasScaler.scaleFactor);
                }
            }
            else if (canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Reference Resolution", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                    NitroxGUILayout.Separator();
                    canvasScaler.referenceResolution = VectorDrawer.DrawVector2(canvasScaler.referenceResolution);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Screen Match Mode", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                    NitroxGUILayout.Separator();
                    canvasScaler.screenMatchMode = NitroxGUILayout.EnumPopup(canvasScaler.screenMatchMode);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Match", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                    NitroxGUILayout.Separator();
                    float newMatchValue = NitroxGUILayout.FloatField(canvasScaler.matchWidthOrHeight);
                    canvasScaler.matchWidthOrHeight = Mathf.Max(0, Mathf.Min(newMatchValue, 1));
                }
            }
            else
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Physical Unit", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                    NitroxGUILayout.Separator();
                    canvasScaler.physicalUnit = NitroxGUILayout.EnumPopup(canvasScaler.physicalUnit);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Fallback Screen DPI", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                    NitroxGUILayout.Separator();
                    canvasScaler.matchWidthOrHeight = NitroxGUILayout.FloatField(canvasScaler.fallbackScreenDPI);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Default Sprite DPI", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                    NitroxGUILayout.Separator();
                    float newDefaultSpriteDPI = NitroxGUILayout.FloatField(canvasScaler.defaultSpriteDPI);
                    canvasScaler.defaultSpriteDPI = Mathf.Max(1, newDefaultSpriteDPI);
                }
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Reference Pixels Per Unit", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            canvasScaler.referencePixelsPerUnit = NitroxGUILayout.FloatField(canvasScaler.referencePixelsPerUnit);
        }
    }
}
