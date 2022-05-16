using System;
using NitroxClient.Debuggers.Drawer.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class TextDrawer : IDrawer
{
    private const float LABEL_WIDTH = 150;
    private const float VALUE_WIDTH = 200;

    public Type[] ApplicableTypes { get; } = { typeof(Text) };

    public void Draw(object target)
    {
        switch (target)
        {
            case Text text:
                DrawText(text);
                break;
        }
    }

    private static void DrawText(Text text)
    {
        GUILayout.Label("Text");
        text.text = GUILayout.TextArea(text.text, GUILayout.MaxHeight(100));

        GUILayout.Space(25);
        GUILayout.Label("Character:", "bold");
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Font", GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            GUILayout.TextField(text.font ? text.font.name : "NoFont", GUILayout.Width(VALUE_WIDTH));
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Font Style", GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            GUILayout.TextField(text.fontStyle.ToString(), GUILayout.Width(VALUE_WIDTH));
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Font Size", GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            text.fontSize = NitroxGUILayout.IntField(text.fontSize, VALUE_WIDTH);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Line Spacing", GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            text.lineSpacing = NitroxGUILayout.FloatField(text.lineSpacing, VALUE_WIDTH);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Rich Text", GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            text.supportRichText = NitroxGUILayout.BoolField(text.supportRichText, VALUE_WIDTH);
        }

        GUILayout.Space(25);
        GUILayout.Label("Paragraph:", "bold");
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Alignment", GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            text.alignment = NitroxGUILayout.EnumPopup(text.alignment, VALUE_WIDTH);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Align By Geometry", GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            text.alignByGeometry = NitroxGUILayout.BoolField(text.alignByGeometry, VALUE_WIDTH);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Horizontal Overflow", GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            text.horizontalOverflow = NitroxGUILayout.EnumPopup(text.horizontalOverflow, VALUE_WIDTH);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Vertical Overflow", GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            text.verticalOverflow = NitroxGUILayout.EnumPopup(text.verticalOverflow, VALUE_WIDTH);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Best Fit", GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            text.resizeTextForBestFit = NitroxGUILayout.BoolField(text.resizeTextForBestFit, VALUE_WIDTH);
        }

        GUILayout.Space(25);
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Color", GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            text.color = ColorDrawer.Draw(text.color);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Material", GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            text.material = MaterialDrawer.Draw(text.material);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Raycast Target", GUILayout.Width(LABEL_WIDTH));
            NitroxGUILayout.Separator();
            text.raycastTarget = NitroxGUILayout.BoolField(text.raycastTarget, VALUE_WIDTH);
        }
    }
}
