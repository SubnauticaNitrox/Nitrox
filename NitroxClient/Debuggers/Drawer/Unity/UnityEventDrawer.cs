using System;
using UnityEngine;
using UnityEngine.Events;

namespace NitroxClient.Debuggers.Drawer.Unity;

public class UnityEventDrawer : IDrawer
{
    private const float LABEL_WIDTH = 250;

    public Type[] ApplicableTypes { get; } = { typeof(UnityEvent), typeof(UnityEvent<bool>) };

    public void Draw(object target)
    {
        switch (target)
        {
            case UnityEvent unityEvent:
                DrawUnityEvent(unityEvent);
                break;
            case UnityEvent<bool> unityEventBool:
                DrawUnityEventBool(unityEventBool);
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

        DrawUnityEventBase(unityEvent);
    }

    public static void DrawUnityEventBool(UnityEvent<bool> unityEvent, string name = "NoName")
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label(name, NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Invoke All (true)", GUILayout.Width(100)))
            {
                unityEvent.Invoke(true);
            }
            if (GUILayout.Button("Invoke All (false)", GUILayout.Width(100)))
            {
                unityEvent.Invoke(false);
            }
        }

        DrawUnityEventBase(unityEvent);
    }

    public static void DrawUnityEventBase(UnityEventBase unityEventBase)
    {
        for (int index = 0; index < unityEventBase.GetPersistentEventCount(); index++)
        {
            using (new GUILayout.HorizontalScope())
            {
                NitroxGUILayout.Separator();
                GUILayout.Label(unityEventBase.GetPersistentMethodName(index), NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            }
        }
    }
}
