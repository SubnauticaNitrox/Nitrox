using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Unity;

public class RigidbodyDrawer : IDrawer<Rigidbody>
{
    private readonly VectorDrawer vectorDrawer;
    private const float LABEL_WIDTH = 120;
    private const float VALUE_MAX_WIDTH = 405;

    public RigidbodyDrawer(VectorDrawer vectorDrawer)
    {
        Validate.NotNull(vectorDrawer);

        this.vectorDrawer = vectorDrawer;
    }

    public void Draw(Rigidbody rb)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Mass", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            rb.mass = NitroxGUILayout.FloatField(rb.mass, VALUE_MAX_WIDTH);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Drag", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            rb.drag = NitroxGUILayout.FloatField(rb.drag, VALUE_MAX_WIDTH);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Angular Drag", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            rb.angularDrag = NitroxGUILayout.FloatField(rb.angularDrag, VALUE_MAX_WIDTH);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Use Gravity");
            NitroxGUILayout.Separator();
            rb.useGravity = NitroxGUILayout.BoolField(rb.useGravity);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Is Kinematic");
            NitroxGUILayout.Separator();
            rb.isKinematic = NitroxGUILayout.BoolField(rb.isKinematic);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Interpolate", GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            rb.interpolation = NitroxGUILayout.EnumPopup(rb.interpolation, NitroxGUILayout.VALUE_WIDTH);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Collision Detection", GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            rb.collisionDetectionMode = NitroxGUILayout.EnumPopup(rb.collisionDetectionMode, NitroxGUILayout.VALUE_WIDTH);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Freeze Rotation");
            NitroxGUILayout.Separator();
            rb.freezeRotation = NitroxGUILayout.BoolField(rb.freezeRotation);
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Velocity", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            vectorDrawer.Draw(rb.velocity, new VectorDrawer.DrawOptions(VALUE_MAX_WIDTH));
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Angular Velocity", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            vectorDrawer.Draw(rb.angularVelocity, new VectorDrawer.DrawOptions(VALUE_MAX_WIDTH));
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Detect Collisions", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            rb.detectCollisions = NitroxGUILayout.BoolField(rb.detectCollisions, NitroxGUILayout.VALUE_WIDTH);
        }
    }
}
