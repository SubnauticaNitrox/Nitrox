using System;
using NitroxClient.Debuggers.Drawer.Unity;
using NitroxModel.Helper;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class GridLayoutGroupDrawer : IDrawer<GridLayoutGroup>
{
    private readonly VectorDrawer vectorDrawer;
    private readonly RectDrawer rectDrawer;

    public GridLayoutGroupDrawer(VectorDrawer vectorDrawer, RectDrawer rectDrawer)
    {
        Validate.NotNull(vectorDrawer);
        Validate.NotNull(rectDrawer);

        this.vectorDrawer = vectorDrawer;
        this.rectDrawer = rectDrawer;
    }

    public void Draw(GridLayoutGroup gridLayoutGroup)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Padding", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            gridLayoutGroup.padding = rectDrawer.Draw(gridLayoutGroup.padding);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Cell Size", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            gridLayoutGroup.cellSize = vectorDrawer.Draw(gridLayoutGroup.cellSize);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Spacing", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            gridLayoutGroup.spacing = vectorDrawer.Draw(gridLayoutGroup.spacing);
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
