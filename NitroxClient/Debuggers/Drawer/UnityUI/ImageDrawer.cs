using System;
using NitroxClient.Debuggers.Drawer.Unity;
using NitroxModel.Helper;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class ImageDrawer : IDrawer<Image>, IDrawer<RawImage>
{
    private readonly ColorDrawer colorDrawer;
    private readonly MaterialDrawer materialDrawer;
    private readonly RectDrawer rectDrawer;

    public ImageDrawer(ColorDrawer colorDrawer, MaterialDrawer materialDrawer, RectDrawer rectDrawer)
    {
        Validate.NotNull(colorDrawer);
        Validate.NotNull(materialDrawer);
        Validate.NotNull(rectDrawer);

        this.colorDrawer = colorDrawer;
        this.materialDrawer = materialDrawer;
        this.rectDrawer = rectDrawer;
    }

    public static void DrawTexture(Texture texture)
    {
        GUIStyle style = new("box") { fixedHeight = texture.height * (250f / texture.width), fixedWidth = 250 };
        GUILayout.Box(texture, style);
    }

    public void Draw(Image image)
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
            image.color = colorDrawer.Draw(image.color);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Material", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            image.material = materialDrawer.Draw(image.material);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Raycast Target", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            image.raycastTarget = NitroxGUILayout.BoolField(image.raycastTarget);
        }
    }

    public void Draw(RawImage rawImage)
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
            rawImage.color = colorDrawer.Draw(rawImage.color);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Material", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            rawImage.material = materialDrawer.Draw(rawImage.material);
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
            rawImage.uvRect = rectDrawer.Draw(rawImage.uvRect);
        }
    }
}
