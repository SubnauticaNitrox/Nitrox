using System;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class MaskDrawer : IDrawer<Mask>, IDrawer<RectMask2D>
{
    public Type[] ApplicableTypes { get; } = { typeof(Mask), typeof(RectMask2D) };

    public void Draw(Mask mask)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Show Mask Graphic", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            mask.showMaskGraphic = NitroxGUILayout.BoolField(mask.showMaskGraphic);
        }
    }

    public void Draw(RectMask2D target)
    {
        // RectMask2D has no fields in the editor.
    }
}
