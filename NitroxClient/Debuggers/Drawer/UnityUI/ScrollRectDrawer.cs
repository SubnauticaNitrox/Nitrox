using System;
using NitroxModel.Core;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class ScrollRectDrawer : IDrawer
{
    public Type[] ApplicableTypes { get; } = { typeof(ScrollRect) };

    public void Draw(object target)
    {
        switch (target)
        {
            case ScrollRect scrollRect:
                DrawScrollRect(scrollRect);
                break;
        }
    }

    private static void DrawScrollRect(ScrollRect scrollRect)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Content", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                NitroxServiceLocator.Cache<SceneDebugger>.Value.JumpToComponent(scrollRect.content);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Horizontal", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            scrollRect.horizontal = NitroxGUILayout.BoolField(scrollRect.horizontal);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Vertical", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            scrollRect.vertical = NitroxGUILayout.BoolField(scrollRect.vertical);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("MovementType", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            scrollRect.movementType = NitroxGUILayout.EnumPopup(scrollRect.movementType);
        }

        if (scrollRect.movementType == ScrollRect.MovementType.Elastic)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Elasticity", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                NitroxGUILayout.Separator();
                scrollRect.elasticity = NitroxGUILayout.FloatField(scrollRect.elasticity);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Inertia", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            scrollRect.inertia = NitroxGUILayout.BoolField(scrollRect.inertia);
        }

        if (scrollRect.inertia)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Deceleration Rate", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                NitroxGUILayout.Separator();
                scrollRect.decelerationRate = NitroxGUILayout.FloatField(scrollRect.decelerationRate);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Scroll Sensitivity", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            scrollRect.scrollSensitivity = NitroxGUILayout.FloatField(scrollRect.scrollSensitivity);
        }

        GUILayout.Space(8f);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Viewport", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                NitroxServiceLocator.Cache<SceneDebugger>.Value.JumpToComponent(scrollRect.viewport);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Horizontal Scrollbar", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                NitroxServiceLocator.Cache<SceneDebugger>.Value.JumpToComponent(scrollRect.horizontalScrollbar);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Vertical Scrollbar", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                NitroxServiceLocator.Cache<SceneDebugger>.Value.JumpToComponent(scrollRect.verticalScrollbar);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("On Value Changed", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            GUILayout.Button("Unsupported", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
        }
    }
}
