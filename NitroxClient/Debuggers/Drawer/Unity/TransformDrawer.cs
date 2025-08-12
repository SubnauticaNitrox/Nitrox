using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Unity;

public class TransformDrawer : IDrawer<Transform>
{
    private readonly SceneDebugger sceneDebugger;
    private readonly VectorDrawer vectorDrawer;
    private const float LABEL_WIDTH = 100;
    private const float VECTOR_MAX_WIDTH = 405;

    private bool showGlobal;

    public TransformDrawer(SceneDebugger sceneDebugger, VectorDrawer vectorDrawer)
    {
        Validate.NotNull(sceneDebugger);
        Validate.NotNull(vectorDrawer);

        this.sceneDebugger = sceneDebugger;
        this.vectorDrawer = vectorDrawer;
    }

    public void Draw(Transform transform)
    {
        using (new GUILayout.VerticalScope())
        {
            if (showGlobal)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Global Position", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                    NitroxGUILayout.Separator();
                    vectorDrawer.Draw(transform.position, new VectorDrawer.DrawOptions(VECTOR_MAX_WIDTH));
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Global Rotation", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                    NitroxGUILayout.Separator();
                    vectorDrawer.Draw(transform.rotation.eulerAngles, new VectorDrawer.DrawOptions(VECTOR_MAX_WIDTH));
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Lossy Scale", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                    NitroxGUILayout.Separator();
                    vectorDrawer.Draw(transform.lossyScale, new VectorDrawer.DrawOptions(VECTOR_MAX_WIDTH));
                }
            }
            else
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Local  Position", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                    NitroxGUILayout.Separator();
                    transform.localPosition = vectorDrawer.Draw(transform.localPosition, new VectorDrawer.DrawOptions(VECTOR_MAX_WIDTH));
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Local  Rotation", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                    NitroxGUILayout.Separator();
                    transform.localRotation = Quaternion.Euler(vectorDrawer.Draw(transform.localRotation.eulerAngles, new VectorDrawer.DrawOptions(VECTOR_MAX_WIDTH)));
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Local  Scale", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                    NitroxGUILayout.Separator();
                    transform.localScale = vectorDrawer.Draw(transform.localScale, new VectorDrawer.DrawOptions(VECTOR_MAX_WIDTH));
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
                            sceneDebugger.JumpToComponent(transform.parent);
                        }
                        UnityEngine.Object.Destroy(transform.gameObject);
                    }
                }
                if (GUILayout.Button("Goto", GUILayout.MaxWidth(75)) && Player.main)
                {
                    SubRoot subRoot = transform.GetComponentInParent<SubRoot>(true);
#if SUBNAUTICA
                    Player.main.SetCurrentSub(subRoot, true);
#elif BELOWZERO
                    Player.main.SetCurrentSub(subRoot);
#endif
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
