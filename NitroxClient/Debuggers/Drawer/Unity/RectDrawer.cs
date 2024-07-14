using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Unity;

public class RectDrawer : IEditorDrawer<Rect, RectDrawer.DrawOptions>, IEditorDrawer<RectOffset>
{
    private const float MAX_WIDTH = 400;

    public Rect Draw(Rect rect, DrawOptions options)
    {
        options ??= new DrawOptions();
        var (valueWidth, maxWidth) = (options.Width, options.MaxWidth);

        using (new GUILayout.HorizontalScope(GUILayout.MaxWidth(maxWidth)))
        {
            using (new GUILayout.VerticalScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("X:", NitroxGUILayout.DrawerLabel);
                    rect.x = NitroxGUILayout.FloatField(rect.x, valueWidth);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Y:", NitroxGUILayout.DrawerLabel);
                    rect.y = NitroxGUILayout.FloatField(rect.y, valueWidth);
                }
            }

            using (new GUILayout.VerticalScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("W:", NitroxGUILayout.DrawerLabel);
                    rect.width = NitroxGUILayout.FloatField(rect.width, valueWidth);
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("H:", NitroxGUILayout.DrawerLabel);
                    rect.height = NitroxGUILayout.FloatField(rect.height, valueWidth);
                }
            }
        }

        return rect;
    }

    public Rect Draw(Rect rect)
    {
        return Draw(rect, null);
    }

    public RectOffset Draw(RectOffset rect, DrawOptions options)
    {
        options ??= new DrawOptions(Width: MAX_WIDTH);

        float valueWidth = options.MaxWidth / 4 - 6;
        using (new GUILayout.HorizontalScope(GUILayout.MaxWidth(options.MaxWidth)))
        {
            rect.left = NitroxGUILayout.IntField(rect.left, valueWidth);
            NitroxGUILayout.Separator();
            rect.right = NitroxGUILayout.IntField(rect.right, valueWidth);
            NitroxGUILayout.Separator();
            rect.top = NitroxGUILayout.IntField(rect.top, valueWidth);
            NitroxGUILayout.Separator();
            rect.bottom = NitroxGUILayout.IntField(rect.bottom, valueWidth);
        }
        return rect;
    }

    public RectOffset Draw(RectOffset rect)
    {
        return Draw(rect, null);
    }

    public record DrawOptions(float Width = 100, float MaxWidth = 215);
}
