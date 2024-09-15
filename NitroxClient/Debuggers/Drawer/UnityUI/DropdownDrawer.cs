using System;
using NitroxModel.Core;
using NitroxModel.Helper;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class DropdownDrawer : IDrawer<Dropdown>
{
    private readonly SelectableDrawer selectableDrawer;
    private readonly SceneDebugger sceneDebugger;

    public DropdownDrawer(SceneDebugger sceneDebugger, SelectableDrawer selectableDrawer)
    {
        Validate.NotNull(sceneDebugger);
        Validate.NotNull(selectableDrawer);

        this.selectableDrawer = selectableDrawer;
        this.sceneDebugger = sceneDebugger;
    }

    public void Draw(Dropdown dropdown)
    {
        selectableDrawer.Draw(dropdown);

        GUILayout.Space(NitroxGUILayout.DEFAULT_SPACE);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Template", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                sceneDebugger.JumpToComponent(dropdown.template);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Caption Text", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                sceneDebugger.JumpToComponent(dropdown.captionText);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Caption Image", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                sceneDebugger.JumpToComponent(dropdown.captionImage);
            }
        }

        GUILayout.Space(NitroxGUILayout.DEFAULT_SPACE);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Item Text", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                sceneDebugger.JumpToComponent(dropdown.itemText);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Item Image", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                sceneDebugger.JumpToComponent(dropdown.itemImage);
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
