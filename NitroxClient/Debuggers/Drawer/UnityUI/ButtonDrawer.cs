using System;
using NitroxClient.Debuggers.Drawer.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class ButtonDrawer : IDrawer
{
    public Type[] ApplicableTypes { get; } = { typeof(Button) };

    public void Draw(object target)
    {
        switch (target)
        {
            case Button button:
                DrawButton(button);
                break;
        }
    }

    private static void DrawButton(Button button)
    {
        SelectableDrawer.DrawSelectable(button);
        GUILayout.Space(10);
        UnityEventDrawer.DrawUnityEvent(button.onClick, "OnClick()");
    }
}
