using System;
using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Unity;

public class RectDrawer : IStructDrawer
{
    public Type[] ApplicableTypes { get; } = { typeof(Rect) };

    public object Draw(object target)
    {
        return target switch
        {
            Rect rect => DrawRect(rect),
            _ => null
        };
    }

    public static Rect DrawRect(Rect rect, float valueWidth = 100, float maxWidth = 215)
    {
        using (new GUILayout.HorizontalScope(GUILayout.MaxWidth(maxWidth)))
        {
            using (new GUILayout.VerticalScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("X:", NitroxGUILayout.DrawerLabel);
                    rect.x = NitroxGUILayout.FloatField(rect.x, valueWidth);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Y:", NitroxGUILayout.DrawerLabel);
                    rect.y = NitroxGUILayout.FloatField(rect.y, valueWidth);
                }
            }

            using (new GUILayout.VerticalScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("W:", NitroxGUILayout.DrawerLabel);
                    rect.width = NitroxGUILayout.FloatField(rect.width, valueWidth);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("H:", NitroxGUILayout.DrawerLabel);
                    rect.height = NitroxGUILayout.FloatField(rect.height, valueWidth);
                }
            }
        }

        return rect;
    }
}
