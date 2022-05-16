using System;
using NitroxClient.Debuggers.Drawer.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class HorizontalOrVerticalLayoutGroupDrawer : IDrawer
{
    public Type[] ApplicableTypes { get; } = { typeof(HorizontalLayoutGroup), typeof(VerticalLayoutGroup) };

    public void Draw(object target)
    {
        switch (target)
        {
            case HorizontalLayoutGroup horizontalLayoutGroup:
                DrawLayoutGroup(horizontalLayoutGroup);
                break;
            case VerticalLayoutGroup verticalLayoutGroup:
                DrawLayoutGroup(verticalLayoutGroup);
                break;
        }
    }

    private static void DrawLayoutGroup(HorizontalOrVerticalLayoutGroup layoutGroup)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Padding", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            Tuple<int, int, int, int> padding = VectorDrawer.DrawInt4(layoutGroup.padding.left, layoutGroup.padding.right,
                                                                      layoutGroup.padding.top, layoutGroup.padding.bottom);

            layoutGroup.padding.left = padding.Item1;
            layoutGroup.padding.right = padding.Item2;
            layoutGroup.padding.top = padding.Item3;
            layoutGroup.padding.bottom = padding.Item4;
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Spacing", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            layoutGroup.spacing = NitroxGUILayout.FloatField(layoutGroup.spacing);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Child Alignment", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            layoutGroup.childAlignment = NitroxGUILayout.EnumPopup(layoutGroup.childAlignment);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Control Child Size", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            layoutGroup.childControlWidth = NitroxGUILayout.BoolField(layoutGroup.childControlWidth, "Width");
            layoutGroup.childControlHeight = NitroxGUILayout.BoolField(layoutGroup.childControlHeight, "Height");
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Use Child Scale", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            layoutGroup.childScaleWidth = NitroxGUILayout.BoolField(layoutGroup.childScaleWidth, "Width");
            layoutGroup.childScaleHeight = NitroxGUILayout.BoolField(layoutGroup.childScaleHeight, "Height");
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Child Force Expand", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            layoutGroup.childForceExpandWidth = NitroxGUILayout.BoolField(layoutGroup.childForceExpandWidth, "Width");
            layoutGroup.childForceExpandHeight = NitroxGUILayout.BoolField(layoutGroup.childForceExpandHeight, "Height");
        }
    }
}
