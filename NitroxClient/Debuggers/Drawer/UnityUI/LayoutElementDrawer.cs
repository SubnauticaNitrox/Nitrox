using System;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class LayoutElementDrawer : IDrawer
{
    private const float LABEL_WIDTH = 100;
    public Type[] ApplicableTypes { get; } = { typeof(LayoutElement) };

    public void Draw(object target)
    {
        switch (target)
        {
            case LayoutElement layoutElement:
                DrawLayoutElement(layoutElement);
                break;
        }
    }

    private static void DrawLayoutElement(LayoutElement layoutElement)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Ignore Layout", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            layoutElement.ignoreLayout = NitroxGUILayout.BoolField(layoutElement.ignoreLayout);
        }

        GUILayout.Space(8f);

        layoutElement.minWidth = DrawToggleableFloat("Min Width", layoutElement.minWidth);
        layoutElement.minHeight = DrawToggleableFloat("Min Height", layoutElement.minHeight);

        layoutElement.preferredWidth = DrawToggleableFloat("Preferred Width", layoutElement.preferredWidth);
        layoutElement.preferredHeight = DrawToggleableFloat("Preferred Height", layoutElement.preferredHeight);

        layoutElement.flexibleWidth = DrawToggleableFloat("Flexible Width", layoutElement.flexibleWidth);
        layoutElement.flexibleHeight = DrawToggleableFloat("Flexible Height", layoutElement.flexibleHeight);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Layout Priority", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            layoutElement.layoutPriority = NitroxGUILayout.IntField(layoutElement.layoutPriority);
        }
    }

    private static float DrawToggleableFloat(string name, float value)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label(name, NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();

            bool active = value >= 0;
            if (GUILayout.Button(active ? "X" : " ", GUILayout.Width(25)))
            {
                value = active ? -1 : 0;
            }

            return Math.Max(-1, NitroxGUILayout.FloatField(value));
        }
    }
}
