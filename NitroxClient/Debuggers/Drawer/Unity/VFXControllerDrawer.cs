using UnityEngine;
using static NitroxClient.Debuggers.Drawer.NitroxGUILayout;

namespace NitroxClient.Debuggers.Drawer.Unity;

public class VFXControllerDrawer : IDrawer<VFXController>
{
    private readonly VectorDrawer vectorDrawer;
    private readonly SceneDebugger sceneDebugger;
    private const float LABEL_WIDTH = 120;
    private const float VALUE_MAX_WIDTH = 405;
    private const float VECTOR_MAX_WIDTH = 405;

    public VFXControllerDrawer(VectorDrawer vectorDrawer, SceneDebugger sceneDebugger)
    {
        this.vectorDrawer = vectorDrawer;
        this.sceneDebugger = sceneDebugger;
    }

    public void Draw(VFXController vfxController)
    {
        GUILayout.Label("Emitters");
        GUILayout.Space(10);

        for (int i = 0; i < vfxController.emitters.Length; i++)
        {
            using (new BackgroundColorScope(Color.green))
            using (new GUILayout.VerticalScope())
            {
                Draw(vfxController, vfxController.emitters[i], i);
            }

            GUILayout.Space(10);
        }
    }

    public void Draw(VFXController vfxController, VFXController.VFXEmitter emitter, int index)
    {
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button($"Play n°{index}"))
            {
                vfxController.Play(index);
            }
            if (GUILayout.Button($"Stop n°{index}"))
            {
                vfxController.Stop(index);
            }
        }

        GUILayout.Space(5);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Spawn on play", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            emitter.spawnOnPlay = NitroxGUILayout.BoolField(emitter.spawnOnPlay, VALUE_MAX_WIDTH);
        }
        
        GUILayout.Space(5);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Fx", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (emitter.fx)
            {
                if (GUILayout.Button(emitter.fx.name, GUILayout.Width(NitroxGUILayout.VALUE_WIDTH)))
                {
                    sceneDebugger.UpdateSelectedObject(emitter.fx);
                }
            }
            else
            {
                GUILayout.Box("Field is null", GUILayout.Width(NitroxGUILayout.VALUE_WIDTH));
            }
        }

        GUILayout.Space(5);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Position Offset", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            emitter.posOffset = vectorDrawer.Draw(emitter.posOffset, new VectorDrawer.DrawOptions(VECTOR_MAX_WIDTH));
        }

        GUILayout.Space(5);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Euler Offset", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            emitter.eulerOffset = vectorDrawer.Draw(emitter.eulerOffset, new VectorDrawer.DrawOptions(VECTOR_MAX_WIDTH));
        }

        GUILayout.Space(5);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Instance GO", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (emitter.instanceGO)
            {
                if (GUILayout.Button(emitter.instanceGO.name, GUILayout.Width(NitroxGUILayout.VALUE_WIDTH)))
                {
                    sceneDebugger.UpdateSelectedObject(emitter.instanceGO);
                }
            }
            else
            {
                GUILayout.Box("Field is null", GUILayout.Width(NitroxGUILayout.VALUE_WIDTH));
            }
        }
    }
}
