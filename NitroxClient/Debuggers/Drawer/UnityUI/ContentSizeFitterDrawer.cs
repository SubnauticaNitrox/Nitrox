using System;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class ContentSizeFitterDrawer : IDrawer
{
    public Type[] ApplicableTypes { get; } = { typeof(ContentSizeFitter) };

    public void Draw(object target)
    {
        switch (target)
        {
            case ContentSizeFitter contentSizeFitter:
                DrawContentSizeFitter(contentSizeFitter);
                break;
        }
    }

    private static void DrawContentSizeFitter(ContentSizeFitter contentSizeFitter)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Horizontal Fit", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            contentSizeFitter.horizontalFit = NitroxGUILayout.EnumPopup(contentSizeFitter.horizontalFit);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Vertical Fit", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            contentSizeFitter.verticalFit = NitroxGUILayout.EnumPopup(contentSizeFitter.verticalFit);
        }
    }
}
