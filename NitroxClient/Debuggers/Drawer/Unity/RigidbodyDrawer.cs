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
        private const float VECTOR_MAX_WIDTH = 405;

        public Type[] ApplicableTypes => new[] { typeof(Rigidbody) };

        public void Draw(object target)
        {
            Rigidbody rb = (Rigidbody)target;            

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Velocity", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                NitroxGUILayout.Separator();
                VectorDrawer.DrawVector3(rb.velocity, VECTOR_MAX_WIDTH);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Angular Velocity", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                NitroxGUILayout.Separator();
                VectorDrawer.DrawVector3(rb.velocity, VECTOR_MAX_WIDTH);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Mass", NitroxGUILayout.DrawerLabel, GUILayout.Width(LABEL_WIDTH));
                if (float.TryParse(GUILayout.TextField(rb.mass.ToString()), out float newMass))
                {
                    rb.mass = newMass;
                }
            }

            rb.useGravity = GUILayout.Toggle(rb.useGravity, "Use Gravity", GUILayout.Width(LABEL_WIDTH));

            rb.isKinematic = GUILayout.Toggle(rb.isKinematic, "Is Kinematic", GUILayout.Width(LABEL_WIDTH));

            rb.freezeRotation = GUILayout.Toggle(rb.freezeRotation, "Freeze Rotation", GUILayout.Width(LABEL_WIDTH));
        }
    }
}
