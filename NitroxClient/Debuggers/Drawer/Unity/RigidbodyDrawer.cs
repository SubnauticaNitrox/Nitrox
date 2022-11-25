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

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Use Gravity");
                NitroxGUILayout.Separator();
                rb.useGravity = NitroxGUILayout.BoolField(rb.useGravity);
            }

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
                rb.interpolation = NitroxGUILayout.EnumPopup(rb.interpolation, VALUE_MAX_WIDTH);
            }

            GUILayout.Space(10);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Collision Detection", GUILayout.Width(LABEL_WIDTH));
                NitroxGUILayout.Separator();
                rb.collisionDetectionMode = NitroxGUILayout.EnumPopup(rb.collisionDetectionMode, VALUE_MAX_WIDTH);
            }

            GUILayout.Space(10);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Freeze Rotation");
                NitroxGUILayout.Separator();
                rb.freezeRotation = NitroxGUILayout.BoolField(rb.freezeRotation);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Velocity", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                NitroxGUILayout.Separator();
                VectorDrawer.DrawVector3Label(rb.velocity, VALUE_MAX_WIDTH);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Angular Velocity", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                NitroxGUILayout.Separator();
                VectorDrawer.DrawVector3Label(rb.angularVelocity, VALUE_MAX_WIDTH);
            }
        }
    }
}
