using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Unity;

public class FixedJointDrawer : IDrawer<FixedJoint>
{
    private readonly SceneDebugger sceneDebugger;
    private readonly VectorDrawer vectorDrawer;
    private const float LABEL_WIDTH = 175;

    public FixedJointDrawer(SceneDebugger sceneDebugger, VectorDrawer vectorDrawer)
    {
        this.sceneDebugger = sceneDebugger;
        this.vectorDrawer = vectorDrawer;
    }

    public void Draw(FixedJoint fixedJoint)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Connected Body", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (fixedJoint.connectedBody)
            {
                if (GUILayout.Button("Goto"))
                {
                    sceneDebugger.JumpToComponent(fixedJoint.connectedBody);
                }
                if (GUILayout.Button("Disconnect"))
                {
                    fixedJoint.connectedBody = null;
                }
            }
            else
            {
                GUILayout.Box("Field is null", GUILayout.Width(NitroxGUILayout.VALUE_WIDTH));
            }
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Axis", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            fixedJoint.axis = vectorDrawer.Draw(fixedJoint.axis);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Anchor", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            fixedJoint.anchor = vectorDrawer.Draw(fixedJoint.anchor);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Connected Anchor", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            fixedJoint.connectedAnchor = vectorDrawer.Draw(fixedJoint.connectedAnchor);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Connected Force", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            vectorDrawer.Draw(fixedJoint.currentForce);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Current Torque", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            vectorDrawer.Draw(fixedJoint.currentTorque);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Enable Collision", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button(fixedJoint.enableCollision.ToString(), GUILayout.Width(NitroxGUILayout.VALUE_WIDTH)))
            {
                fixedJoint.enableCollision = !fixedJoint.enableCollision;
            }
        }
    }
}
