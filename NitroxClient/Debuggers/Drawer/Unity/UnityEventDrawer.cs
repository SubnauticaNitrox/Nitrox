using System;
using UnityEngine;
using UnityEngine.Events;

namespace NitroxClient.Debuggers.Drawer.Unity;

public class UnityEventDrawer : IDrawer
{
    private const float LABEL_WIDTH = 250;

    public Type[] ApplicableTypes { get; } = { typeof(UnityEvent) };

    public void Draw(object target)
    {
        switch (target)
        {
            case UnityEvent unityEvent:
                DrawUnityEvent(unityEvent);
                break;
        }
    }

    public static void DrawUnityEvent(UnityEvent unityEvent, string name = "NoName")
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label(name, NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Invoke All", GUILayout.Width(100)))
            {
                unityEvent.Invoke();
            }
        }

        for (int index = 0; index < unityEvent.GetPersistentEventCount(); index++)
        {
            using (new GUILayout.HorizontalScope())
            {
                NitroxGUILayout.Separator();
                GUILayout.Label(unityEvent.GetPersistentMethodName(index), NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            }
        }
    }
}
