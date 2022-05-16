using System;
using NitroxClient.Debuggers.Drawer.Unity;
using NitroxModel.Core;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class InputFieldDrawer : IDrawer
{
    public Type[] ApplicableTypes { get; } = { typeof(InputField) };

    public void Draw(object target)
    {
        switch (target)
        {
            case InputField inputField:
                DrawInputField(inputField);
                break;
        }
    }

    private static void DrawInputField(InputField inputField)
    {
        SelectableDrawer.DrawSelectable(inputField);

        GUILayout.Space(NitroxGUILayout.DEFAULT_SPACE);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Text Component", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                NitroxServiceLocator.Cache<SceneDebugger>.Value.JumpToComponent(inputField.textComponent);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Text", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            inputField.text = GUILayout.TextArea(inputField.text, GUILayout.MaxHeight(100));
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Character Limit", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            inputField.characterLimit = NitroxGUILayout.IntField(inputField.characterLimit);
        }

        GUILayout.Space(NitroxGUILayout.DEFAULT_SPACE);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Content Type", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            inputField.contentType = NitroxGUILayout.EnumPopup(inputField.contentType);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Line Type", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            inputField.lineType = NitroxGUILayout.EnumPopup(inputField.lineType);
        }

        GUILayout.Space(NitroxGUILayout.DEFAULT_SPACE);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Placeholder", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                NitroxServiceLocator.Cache<SceneDebugger>.Value.JumpToComponent(inputField.placeholder);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Caret Blink Rate", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            inputField.caretBlinkRate = NitroxGUILayout.SliderField(inputField.caretBlinkRate, 0f, 4f);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Caret Width", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            inputField.caretWidth = NitroxGUILayout.SliderField(inputField.caretWidth, 1, 5);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Custom Caret Color", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            inputField.customCaretColor = NitroxGUILayout.BoolField(inputField.customCaretColor);
        }

        if (inputField.customCaretColor)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Caret Color", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
                NitroxGUILayout.Separator();
                inputField.caretColor = ColorDrawer.Draw(inputField.caretColor);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Selection Color", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            inputField.selectionColor = ColorDrawer.Draw(inputField.selectionColor);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Hide Mobile Input", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            inputField.shouldHideMobileInput = NitroxGUILayout.BoolField(inputField.shouldHideMobileInput);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Read Only", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            inputField.readOnly = NitroxGUILayout.BoolField(inputField.readOnly);
        }

        GUILayout.Space(NitroxGUILayout.DEFAULT_SPACE);

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("On Value Changed", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            GUILayout.Button("Unsupported", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("On End Edit", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            GUILayout.Button("Unsupported", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
        }
    }
}
