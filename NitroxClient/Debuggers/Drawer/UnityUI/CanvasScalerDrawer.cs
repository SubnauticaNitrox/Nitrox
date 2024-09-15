using NitroxClient.Debuggers.Drawer.Unity;
using NitroxModel.Helper;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class CanvasScalerDrawer : IDrawer<CanvasScaler>
{
    private readonly VectorDrawer vectorDrawer;

    public CanvasScalerDrawer(VectorDrawer vectorDrawer)
    {
        Validate.NotNull(vectorDrawer);

        this.vectorDrawer = vectorDrawer;
    }

    public void Draw(CanvasScaler canvasScaler)
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
                    canvasScaler.referenceResolution = vectorDrawer.Draw(canvasScaler.referenceResolution);
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
                    canvasScaler.matchWidthOrHeight = UnityEngine.Mathf.Max(0, UnityEngine.Mathf.Min(newMatchValue, 1));
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
                    canvasScaler.defaultSpriteDPI = UnityEngine.Mathf.Max(1, newDefaultSpriteDPI);
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
