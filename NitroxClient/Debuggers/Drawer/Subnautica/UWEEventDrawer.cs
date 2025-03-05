using System.Collections.Generic;
using UnityEngine;
using UWE;

namespace NitroxClient.Debuggers.Drawer.Subnautica;

public class UWEEventDrawer : IDrawer<Event<float>>, IDrawer<Event<PowerRelay>>
{
    private const float LABEL_WIDTH = 250;

    public void Draw(Event<float> uweEvent) => UWEEventDrawer.Draw(uweEvent);
    public void Draw(Event<PowerRelay> uweEvent) => UWEEventDrawer.Draw(uweEvent);

    private static void Draw<T>(Event<T> uweEvent)
    {
        using GUILayout.VerticalScope scope = new();

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Triggering", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            uweEvent.triggering = NitroxGUILayout.BoolField(uweEvent.triggering);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Handlers", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            DrawUweEventHandlerList(uweEvent.handlers);
        }
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("ToRemove", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            DrawUweEventHandlerList(uweEvent.toRemove);
        }
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("HandlersToTrigger", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            DrawUweEventHandlerList(uweEvent.handlersToTrigger);
        }
    }

    private static void DrawUweEventHandlerList<T>(ICollection<Event<T>.Handler> uweEventHandlerList)
    {
        if (uweEventHandlerList == null)
        {
            GUILayout.Label("null", NitroxGUILayout.DrawerLabel);
            return;
        }

        if (uweEventHandlerList.Count == 0)
        {
            GUILayout.Label("empty", NitroxGUILayout.DrawerLabel);
            return;
        }

        foreach (Event<T>.Handler uweEventHandler in uweEventHandlerList)
        {
            using (new GUILayout.HorizontalScope())
            {
                NitroxGUILayout.Separator();
                if (uweEventHandler == null)
                {
                    GUILayout.Label("Handler was null", NitroxGUILayout.DrawerLabel);
                    continue;
                }

                string labelText = uweEventHandler.obj ? $"{uweEventHandler.obj.GetType().Name}." : string.Empty;
                labelText += uweEventHandler.function;
                GUILayout.Label(labelText, NitroxGUILayout.DrawerLabel);
            }
        }
    }
}
