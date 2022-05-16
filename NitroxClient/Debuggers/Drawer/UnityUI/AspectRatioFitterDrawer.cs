using System;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class AspectRatioFitterDrawer : IDrawer
{
    public Type[] ApplicableTypes { get; } = { typeof(AspectRatioFitter) };

    public void Draw(object target)
    {
        switch (target)
        {
            case AspectRatioFitter aspectRatioFitter:
                DrawAspectRatioFitter(aspectRatioFitter);
                break;
        }
    }

    private static void DrawAspectRatioFitter(AspectRatioFitter aspectRatioFitter)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Aspect Mode", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            aspectRatioFitter.aspectMode = NitroxGUILayout.EnumPopup(aspectRatioFitter.aspectMode);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Aspect Ratio", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();

            if (aspectRatioFitter.aspectMode == AspectRatioFitter.AspectMode.None)
            {
                NitroxGUILayout.FloatField(aspectRatioFitter.aspectRatio);
            }
            else
            {
                aspectRatioFitter.aspectRatio = NitroxGUILayout.FloatField(aspectRatioFitter.aspectRatio);
            }
        }
    }
}
