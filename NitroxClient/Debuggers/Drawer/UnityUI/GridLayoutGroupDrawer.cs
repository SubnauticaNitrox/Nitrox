using System;
using NitroxClient.Debuggers.Drawer.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class GridLayoutGroupDrawer : IDrawer
{
    public Type[] ApplicableTypes { get; } = { typeof(GridLayoutGroup) };

    public void Draw(object target)
    {
        switch (target)
        {
            case GridLayoutGroup gridLayoutGroup:
                DrawGridLayoutGroup(gridLayoutGroup);
                break;
        }
    }

    private static void DrawGridLayoutGroup(GridLayoutGroup gridLayoutGroup)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Padding", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            Tuple<int, int, int, int> padding = VectorDrawer.DrawInt4(gridLayoutGroup.padding.left, gridLayoutGroup.padding.right,
                                                                      gridLayoutGroup.padding.top, gridLayoutGroup.padding.bottom);

            gridLayoutGroup.padding.left = padding.Item1;
            gridLayoutGroup.padding.right = padding.Item2;
            gridLayoutGroup.padding.top = padding.Item3;
            gridLayoutGroup.padding.bottom = padding.Item4;
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Cell Size", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            gridLayoutGroup.cellSize = VectorDrawer.DrawVector2(gridLayoutGroup.cellSize);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Spacing", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            gridLayoutGroup.spacing = VectorDrawer.DrawVector2(gridLayoutGroup.spacing);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Start Corner", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            gridLayoutGroup.startCorner = NitroxGUILayout.EnumPopup(gridLayoutGroup.startCorner);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Start Axis", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            gridLayoutGroup.startAxis = NitroxGUILayout.EnumPopup(gridLayoutGroup.startAxis);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Child Alignment", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            gridLayoutGroup.childAlignment = NitroxGUILayout.EnumPopup(gridLayoutGroup.childAlignment);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Constraint", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            gridLayoutGroup.constraint = NitroxGUILayout.EnumPopup(gridLayoutGroup.constraint);
        }

        if (gridLayoutGroup.constraint != GridLayoutGroup.Constraint.Flexible)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Constraint Count", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                NitroxGUILayout.Separator();
                gridLayoutGroup.constraintCount = Math.Max(1, NitroxGUILayout.IntField(gridLayoutGroup.constraintCount));
            }
        }
    }
}
