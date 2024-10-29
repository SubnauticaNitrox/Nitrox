using System;
using System.Linq;
using NitroxModel.Helper;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class EventTriggerDrawer : IDrawer<EventTrigger>
{
    private readonly SceneDebugger sceneDebugger;

    public EventTriggerDrawer(SceneDebugger sceneDebugger)
    {
        Validate.NotNull(sceneDebugger);

        this.sceneDebugger = sceneDebugger;
    }

    public void Draw(EventTrigger eventTrigger)
    {
        foreach (EventTrigger.Entry entry in eventTrigger.triggers.OrderBy(x => x.eventID))
        {
            using (new GUILayout.VerticalScope("box"))
            {
                GUILayout.Label($"EventTriggerType.{Enum.GetName(typeof(EventTriggerType), entry.eventID)}");

                for (int i = 0; i < entry.callback.GetPersistentEventCount(); i++)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        object target = entry.callback.GetPersistentTarget(i);

                        switch (target)
                        {
                            case Component component:
                                if (GUILayout.Button($"Jump to {component.name}", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
                                {
                                    sceneDebugger.JumpToComponent(component);
                                }
                                NitroxGUILayout.Separator();
                                GUILayout.TextField($"{component.GetType().Name}.{entry.callback.GetPersistentMethodName(i)}()", GUILayout.ExpandWidth(true), GUILayout.MaxWidth(250));
                                break;
                            default:
                                GUILayout.TextField($"[{target.GetType().Name}]: {target}", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                                NitroxGUILayout.Separator();
                                GUILayout.TextField($"{entry.callback.GetPersistentMethodName(i)}()", GUILayout.ExpandWidth(true), GUILayout.MaxWidth(250));
                                break;
                        }
                    }
                    NitroxGUILayout.Separator();
                }
            }
            NitroxGUILayout.Separator();
        }
    }
}
