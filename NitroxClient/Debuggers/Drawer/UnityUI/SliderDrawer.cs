using System;
using NitroxModel.Core;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class SliderDrawer : IDrawer
{
    public Type[] ApplicableTypes { get; } = { typeof(Slider) };

    public void Draw(object target)
    {
        switch (target)
        {
            case Slider slider:
                DrawSlider(slider);
                break;
        }
    }

    private static void DrawSlider(Slider slider)
    {
        SelectableDrawer.DrawSelectable(slider);

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Fill Rect", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                NitroxServiceLocator.Cache<SceneDebugger>.Value.JumpToComponent(slider.fillRect);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Handle Rect", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                NitroxServiceLocator.Cache<SceneDebugger>.Value.JumpToComponent(slider.handleRect);
            }
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Direction", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            slider.direction = NitroxGUILayout.EnumPopup(slider.direction);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Min Value", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            slider.minValue = NitroxGUILayout.FloatField(slider.minValue);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Max Value", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            slider.maxValue = NitroxGUILayout.FloatField(slider.maxValue);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Whole Numbers", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            slider.wholeNumbers = NitroxGUILayout.BoolField(slider.wholeNumbers);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Value", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (slider.wholeNumbers)
            {
                slider.value = NitroxGUILayout.SliderField((int)slider.value, (int)slider.minValue, (int)slider.maxValue);
            }
            else
            {
                slider.value = NitroxGUILayout.SliderField(slider.value, slider.minValue, slider.maxValue);
            }
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("On Value Changed", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            GUILayout.Button("Unsupported", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
        }
    }
}
