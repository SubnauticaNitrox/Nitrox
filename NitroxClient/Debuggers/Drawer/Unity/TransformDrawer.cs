using System;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Unity;

public class TransformDrawer : IDrawer
{
    private const float LABEL_WIDTH = 100;
    private const float VECTOR_MAX_WIDTH = 405;

    public Type[] ApplicableTypes { get; } = { typeof(Transform) };

    private bool showGlobal;

    public void Draw(object target)
    {
        switch (target)
        {
            case Transform transform:
                DrawTransform(transform);
                break;
        }
    }

    private void DrawTransform(Transform transform)
    {
        using (new GUILayout.VerticalScope())
        {
            if (showGlobal)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Global Position", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                    NitroxGUILayout.Separator();
                    VectorDrawer.DrawVector3(transform.position, VECTOR_MAX_WIDTH);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Global Rotation", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                    NitroxGUILayout.Separator();
                    VectorDrawer.DrawVector3(transform.rotation.eulerAngles, VECTOR_MAX_WIDTH);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Lossy Scale", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                    NitroxGUILayout.Separator();
                    VectorDrawer.DrawVector3(transform.lossyScale, VECTOR_MAX_WIDTH);
                }
            }
            else
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Local  Position", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                    NitroxGUILayout.Separator();
                    transform.localPosition = VectorDrawer.DrawVector3(transform.localPosition, VECTOR_MAX_WIDTH);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Local  Rotation", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                    NitroxGUILayout.Separator();
                    transform.localRotation = Quaternion.Euler(VectorDrawer.DrawVector3(transform.localRotation.eulerAngles, VECTOR_MAX_WIDTH));
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Local  Scale", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                    NitroxGUILayout.Separator();
                    transform.localScale = VectorDrawer.DrawVector3(transform.localScale, VECTOR_MAX_WIDTH);
                }
            }

            GUILayout.Space(5);

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Toggle Local/Global", GUILayout.MaxWidth(125)))
                {
                    showGlobal = !showGlobal;
                }
                if (GUILayout.Button("Destroy GameObject", GUILayout.MaxWidth(150)))
                {
                    if (transform)
                    {
                        if (transform.parent)
                        {
                            NitroxServiceLocator.Cache<SceneDebugger>.Value.JumpToComponent(transform.parent);
                        }
                        GameObject.Destroy(transform.gameObject);
                    }
                }
                if (GUILayout.Button("Goto", GUILayout.MaxWidth(75)) && Player.main)
                {
                    SubRoot subRoot = transform.GetComponentInParent<SubRoot>(true);
                    Player.main.SetCurrentSub(subRoot, true);
                    Player.main.SetPosition(transform.position);
                }
                if (GUILayout.Button($"Set {(transform.gameObject.activeSelf ? "inactive" : "active")}", GUILayout.MaxWidth(125)))
                {
                    transform.gameObject.SetActive(!transform.gameObject.activeSelf);
                }
            }
        }
    }
}
