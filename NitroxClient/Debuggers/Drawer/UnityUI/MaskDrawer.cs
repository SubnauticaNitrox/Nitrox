using System;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class MaskDrawer : IDrawer
{
    public Type[] ApplicableTypes { get; } = { typeof(Mask), typeof(RectMask2D) };

    public void Draw(object target)
    {
        switch (target)
        {
            case Mask mask:
                DrawMask(mask);
                break;
            case RectMask2D: // RectMask2D has no fields in the editor.
                break;
        }
    }

    private static void DrawMask(Mask mask)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Show Mask Graphic", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            mask.showMaskGraphic = NitroxGUILayout.BoolField(mask.showMaskGraphic);
        }
    }
}
