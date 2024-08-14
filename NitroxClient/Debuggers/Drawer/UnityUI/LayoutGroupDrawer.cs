using System;
using NitroxClient.Debuggers.Drawer.Unity;
using NitroxModel.Helper;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class LayoutGroupDrawer : IDrawer<HorizontalLayoutGroup>, IDrawer<VerticalLayoutGroup>
{
    private readonly RectDrawer rectDrawer;

    public LayoutGroupDrawer(RectDrawer rectDrawer)
    {
        Validate.NotNull(rectDrawer);

        this.rectDrawer = rectDrawer;
    }

    public void Draw(HorizontalLayoutGroup target)
    {
        DrawLayoutGroup(target);
    }

    public void Draw(VerticalLayoutGroup target)
    {
        DrawLayoutGroup(target);
    }

    private void DrawLayoutGroup(HorizontalOrVerticalLayoutGroup layoutGroup)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Padding", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            rectDrawer.Draw(layoutGroup.padding);
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
