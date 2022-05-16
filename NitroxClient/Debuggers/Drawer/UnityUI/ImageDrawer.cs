using System;
using NitroxClient.Debuggers.Drawer.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class ImageDrawer : IDrawer
{
    public Type[] ApplicableTypes { get; } = { typeof(Image), typeof(RawImage) };

    public void Draw(object target)
    {
        switch (target)
        {
            case Image image:
                DrawImage(image);
                break;
            case RawImage rawImage:
                DrawRawTexture(rawImage);
                break;
        }
    }

    public static void DrawTexture(Texture texture)
    {
        GUIStyle style = new("box") { fixedHeight = texture.height * (250f / texture.width), fixedWidth = 250 };
        GUILayout.Box(texture, style);
    }

    private static void DrawImage(Image image)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Image", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            DrawTexture(image.mainTexture);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Color", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            image.color = ColorDrawer.Draw(image.color);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Material", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            image.material = MaterialDrawer.Draw(image.material);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Raycast Target", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            image.raycastTarget = NitroxGUILayout.BoolField(image.raycastTarget);
        }
    }

    private static void DrawRawTexture(RawImage rawImage)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Image", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            DrawTexture(rawImage.mainTexture);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Color", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            rawImage.color = ColorDrawer.Draw(rawImage.color);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Material", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            rawImage.material = MaterialDrawer.Draw(rawImage.material);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Raycast Target", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            rawImage.raycastTarget = NitroxGUILayout.BoolField(rawImage.raycastTarget);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("UV Rect", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            rawImage.uvRect = RectDrawer.DrawRect(rawImage.uvRect);
        }
    }
}
