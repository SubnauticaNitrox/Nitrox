using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Unity;

/// <summary>
/// Draws a <see cref="CharacterController"/> component on the gameobjects in the <see cref="SceneDebugger"/>
/// </summary>
public class CharacterControllerDrawer : IDrawer<CharacterController>
{
    private readonly VectorDrawer vectorDrawer;
    private const float LABEL_WIDTH = 120;
    private const float VALUE_MAX_WIDTH = 405;

    public CharacterControllerDrawer(VectorDrawer vectorDrawer)
    {
        Validate.NotNull(vectorDrawer);

        this.vectorDrawer = vectorDrawer;
    }

    public void Draw(CharacterController cc)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Slope Limit (°)", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            cc.slopeLimit = NitroxGUILayout.FloatField(cc.slopeLimit, VALUE_MAX_WIDTH);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Step Offset", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            cc.stepOffset = NitroxGUILayout.FloatField(cc.stepOffset, VALUE_MAX_WIDTH);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Skin Width", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            cc.skinWidth = NitroxGUILayout.FloatField(cc.skinWidth, VALUE_MAX_WIDTH);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Min Move Distance", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            cc.minMoveDistance = NitroxGUILayout.FloatField(cc.minMoveDistance, VALUE_MAX_WIDTH);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Center", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            cc.center = vectorDrawer.Draw(cc.center);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Radius", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            cc.radius = NitroxGUILayout.FloatField(cc.radius, VALUE_MAX_WIDTH);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Height", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            cc.height = NitroxGUILayout.FloatField(cc.height, VALUE_MAX_WIDTH);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Velocity", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            vectorDrawer.Draw(cc.velocity);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Is Grounded", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            NitroxGUILayout.BoolField(cc.isGrounded, VALUE_MAX_WIDTH);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Detect Collisions", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            cc.detectCollisions = NitroxGUILayout.BoolField(cc.detectCollisions, VALUE_MAX_WIDTH);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Enable Overlap Recovery", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH + 30));
            NitroxGUILayout.Separator();
            cc.enableOverlapRecovery = NitroxGUILayout.BoolField(cc.enableOverlapRecovery, VALUE_MAX_WIDTH);
        }
    }
}
