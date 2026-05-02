using Nitrox.Model.Helper;
using UnityEngine;
using UWE;

namespace NitroxClient.Debuggers.Drawer.Unity;

public sealed class BoxColliderDrawer : IDrawer<BoxCollider>
{
    private readonly RigidbodyDrawer rigidbodyDrawer;
    private readonly VectorDrawer vectorDrawer;
    private const float VECTOR_MAX_WIDTH = 405;

    public BoxColliderDrawer(VectorDrawer vectorDrawer, RigidbodyDrawer rigidbodyDrawer)
    {
        Validate.NotNull(vectorDrawer);
        Validate.NotNull(rigidbodyDrawer);

        this.vectorDrawer = vectorDrawer;
        this.rigidbodyDrawer = rigidbodyDrawer;
    }

    public void Draw(BoxCollider target)
    {
        using (new GUILayout.VerticalScope())
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Center", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                NitroxGUILayout.Separator();
                target.center = vectorDrawer.Draw(target.center, new VectorDrawer.DrawOptions(VECTOR_MAX_WIDTH));
            }
    
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Size", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                NitroxGUILayout.Separator();
                target.size = vectorDrawer.Draw(target.size, new VectorDrawer.DrawOptions(VECTOR_MAX_WIDTH));
            }
        }

        GUILayout.Space(10);

        using (new GUILayout.VerticalScope())
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Enabled", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                NitroxGUILayout.Separator();
                target.enabled = NitroxGUILayout.BoolField(target.enabled);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Is Trigger", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                NitroxGUILayout.Separator();
                target.isTrigger = NitroxGUILayout.BoolField(target.isTrigger);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Contact Offset", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                NitroxGUILayout.Separator();
                target.contactOffset = NitroxGUILayout.FloatField(target.contactOffset);
            }
        }

        GUILayout.Space(10);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Attached Rigid Body", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            rigidbodyDrawer.Draw(target.attachedRigidbody);
        }
    }
}
