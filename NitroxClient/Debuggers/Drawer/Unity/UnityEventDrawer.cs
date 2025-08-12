using UnityEngine;
using UnityEngine.Events;

namespace NitroxClient.Debuggers.Drawer.Unity;

public class UnityEventDrawer : IDrawer<UnityEvent, UnityEventDrawer.DrawOptions>, IDrawer<UnityEvent<bool>, UnityEventDrawer.DrawOptions>
{
    private const float LABEL_WIDTH = 250;

    public void Draw(UnityEvent unityEvent, DrawOptions options)
    {
        options ??= new DrawOptions();

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label(options.Name, NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Invoke All", GUILayout.Width(100)))
            {
                unityEvent.Invoke();
            }
        }

        DrawUnityEventBase(unityEvent);
    }

    public void Draw(UnityEvent<bool> unityEvent, DrawOptions options)
    {
        options ??= new DrawOptions();

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label(options.Name, NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
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

    private static void DrawUnityEventBase(UnityEventBase unityEventBase)
    {
        for (int index = 0; index < unityEventBase.GetPersistentEventCount(); index++)
        {
            using (new GUILayout.HorizontalScope())
            {
                NitroxGUILayout.Separator();
                Object target = unityEventBase.GetPersistentTarget(index);
                string labelText = target ? $"{target.GetType().Name}.{unityEventBase.GetPersistentMethodName(index)}()" : $"{unityEventBase.GetPersistentMethodName(index)}()";
                GUILayout.Label(labelText, NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            }
        }
    }

    public record DrawOptions(string Name = "NoName");

    public void Draw(UnityEvent unityEvent) => Draw(unityEvent, null);

    public void Draw(UnityEvent<bool> unityEvent) => Draw(unityEvent, null);
}
