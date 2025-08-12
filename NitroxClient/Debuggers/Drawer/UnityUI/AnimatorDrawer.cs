using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class AnimatorDrawer : IDrawer<Animator>
{
    public void Draw(Animator target)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("updateMode", GUILayout.Width(200));
            target.updateMode = NitroxGUILayout.EnumPopup(target.updateMode, NitroxGUILayout.VALUE_WIDTH);
        }

        GUILayout.Label($"Parameters [{target.parameterCount}]:");
        foreach (AnimatorControllerParameter parameter in target.parameters)
        {
            GUILayout.Space(8);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(parameter.name, GUILayout.Width(200));
                NitroxGUILayout.Separator();
                switch (parameter.type)
                {
                    case AnimatorControllerParameterType.Float:
                        float floatValue = NitroxGUILayout.FloatField(target.GetFloat(parameter.name));

                        target.SetFloat(parameter.name, GUILayout.Button("Reset") ? parameter.defaultFloat : floatValue);
                        break;
                    case AnimatorControllerParameterType.Int:
                        int intValue = NitroxGUILayout.IntField(target.GetInteger(parameter.name));

                        target.SetInteger(parameter.name, GUILayout.Button("Reset") ? parameter.defaultInt : intValue);
                        break;
                    case AnimatorControllerParameterType.Bool:
                        bool boolValue = NitroxGUILayout.BoolField(target.GetBool(parameter.name));

                        target.SetBool(parameter.name, GUILayout.Button("Reset") ? parameter.defaultBool : boolValue);
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        if (GUILayout.Button("Trigger"))
                        {
                            target.SetTrigger(parameter.name);
                        }
                        if (GUILayout.Button("Reset"))
                        {
                            target.ResetTrigger(parameter.name);
                        }
                        break;
                }
            }
        }
    }
}
