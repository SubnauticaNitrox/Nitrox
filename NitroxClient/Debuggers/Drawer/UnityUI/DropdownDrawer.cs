using System;
using NitroxModel.Core;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class DropdownDrawer : IDrawer
{
    public Type[] ApplicableTypes { get; } = { typeof(Dropdown) };

    public void Draw(object target)
    {
        switch (target)
        {
            case Dropdown dropdown:
                DrawDropdown(dropdown);
                break;
        }
    }

    private static void DrawDropdown(Dropdown dropdown)
    {
        SelectableDrawer.DrawSelectable(dropdown);

        GUILayout.Space(NitroxGUILayout.DEFAULT_SPACE);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Template", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                NitroxServiceLocator.Cache<SceneDebugger>.Value.JumpToComponent(dropdown.template);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Caption Text", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                NitroxServiceLocator.Cache<SceneDebugger>.Value.JumpToComponent(dropdown.captionText);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Caption Image", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                NitroxServiceLocator.Cache<SceneDebugger>.Value.JumpToComponent(dropdown.captionImage);
            }
        }

        GUILayout.Space(NitroxGUILayout.DEFAULT_SPACE);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Item Text", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                NitroxServiceLocator.Cache<SceneDebugger>.Value.JumpToComponent(dropdown.itemText);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Item Image", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                NitroxServiceLocator.Cache<SceneDebugger>.Value.JumpToComponent(dropdown.itemImage);
            }
        }

        GUILayout.Space(NitroxGUILayout.DEFAULT_SPACE);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Value", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            dropdown.value = NitroxGUILayout.IntField(dropdown.value);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Alpha Fade Speed", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            dropdown.alphaFadeSpeed = NitroxGUILayout.FloatField(dropdown.alphaFadeSpeed);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Options", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            GUILayout.Button("Unsupported", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("On Value Changed", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            GUILayout.Button("Unsupported", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
        }
    }
}
