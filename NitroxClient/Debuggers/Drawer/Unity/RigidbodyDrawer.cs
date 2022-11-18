using System;
using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Unity
{
    /// <summary>
    /// Draws a Rigidbody component on the gameobjects in the <see cref="SceneDebugger"/>
    /// </summary>
    public class RigidbodyDrawer : IDrawer
    {
        private const float LABEL_WIDTH = 120;
        private const float VALUE_MAX_WIDTH = 405;

        public Type[] ApplicableTypes => new[] { typeof(Rigidbody) };

        public void Draw(object target)
        {
            Rigidbody rb = (Rigidbody)target;

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Mass", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                NitroxGUILayout.Separator();
                rb.mass = NitroxGUILayout.FloatField(rb.mass, VALUE_MAX_WIDTH);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Drag", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                NitroxGUILayout.Separator();
                rb.drag = NitroxGUILayout.FloatField(rb.drag, VALUE_MAX_WIDTH);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Angular Drag", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                NitroxGUILayout.Separator();
                rb.angularDrag = NitroxGUILayout.FloatField(rb.angularDrag, VALUE_MAX_WIDTH);
            }

            rb.useGravity = GUILayout.Toggle(rb.useGravity, "Use Gravity", GUILayout.Width(LABEL_WIDTH));

            rb.isKinematic = GUILayout.Toggle(rb.isKinematic, "Is Kinematic", GUILayout.Width(LABEL_WIDTH));

            rb.interpolation = NitroxGUILayout.EnumPopup(rb.interpolation, LABEL_WIDTH);

            rb.collisionDetectionMode = NitroxGUILayout.EnumPopup(rb.collisionDetectionMode, LABEL_WIDTH);

            rb.freezeRotation = GUILayout.Toggle(rb.freezeRotation, "Freeze Rotation", GUILayout.Width(LABEL_WIDTH));

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Velocity", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                NitroxGUILayout.Separator();
                rb.velocity = VectorDrawer.DrawVector3(rb.velocity, VALUE_MAX_WIDTH);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Angular Velocity", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                NitroxGUILayout.Separator();
                rb.angularVelocity = VectorDrawer.DrawVector3(rb.angularVelocity, VALUE_MAX_WIDTH);
            }
        }
    }
}
