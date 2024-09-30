using NitroxClient.Debuggers.Drawer.Unity;
using NitroxModel.Helper;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Debuggers.Drawer.UnityUI;

public class SelectableDrawer : IDrawer<Selectable>
{
    private readonly SceneDebugger sceneDebugger;
    private readonly ColorDrawer colorDrawer;

    public SelectableDrawer(SceneDebugger sceneDebugger, ColorDrawer colorDrawer)
    {
        Validate.NotNull(sceneDebugger);
        Validate.NotNull(colorDrawer);

        this.sceneDebugger = sceneDebugger;
        this.colorDrawer = colorDrawer;
    }

    public void Draw(Selectable selectable)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Interactable", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            selectable.interactable = NitroxGUILayout.BoolField(selectable.interactable);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Transition", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            selectable.transition = NitroxGUILayout.EnumPopup(selectable.transition);
        }

        switch (selectable.transition)
        {
            case Selectable.Transition.ColorTint:
                DrawTransitionColorTint(selectable);
                break;
            case Selectable.Transition.SpriteSwap:
                DrawTransitionSpriteSwap(selectable);
                break;
            case Selectable.Transition.Animation:
                DrawTransitionAnimation(selectable);
                break;
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Navigation", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            NitroxGUILayout.EnumPopup(selectable.navigation.mode);
        }
    }

    private void DrawTransitionColorTint(Selectable selectable)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Target Graphic", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.VALUE_WIDTH)))
            {
                sceneDebugger.UpdateSelectedObject(selectable.targetGraphic.gameObject);
            }
        }

        Color normalColor, highlightedColor, pressedColor, selectedColor, disabledColor;
        float colorMultiplier, fadeDuration;

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Normal Color", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            normalColor = colorDrawer.Draw(selectable.colors.normalColor);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Highlighted Color", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            highlightedColor = colorDrawer.Draw(selectable.colors.highlightedColor);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Pressed Color", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            pressedColor = colorDrawer.Draw(selectable.colors.pressedColor);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Selected Color", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            selectedColor = colorDrawer.Draw(selectable.colors.selectedColor);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Disabled Color", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            disabledColor = colorDrawer.Draw(selectable.colors.disabledColor);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Color Multiplier", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            colorMultiplier = NitroxGUILayout.SliderField(selectable.colors.colorMultiplier, 1, 5);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Fader Duration", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            fadeDuration = NitroxGUILayout.SliderField(selectable.colors.fadeDuration, 1, 5);
        }

        selectable.colors = new ColorBlock
        {
            normalColor = normalColor,
            highlightedColor = highlightedColor,
            pressedColor = pressedColor,
            selectedColor = selectedColor,
            disabledColor = disabledColor,
            colorMultiplier = colorMultiplier,
            fadeDuration = fadeDuration
        };
    }

    private void DrawTransitionSpriteSwap(Selectable selectable)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Target Graphic", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (GUILayout.Button("Jump to", GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH)))
            {
                sceneDebugger.UpdateSelectedObject(selectable.targetGraphic.gameObject);
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Highlighted Sprite", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (selectable.spriteState.highlightedSprite)
            {
                ImageDrawer.DrawTexture(selectable.spriteState.highlightedSprite.texture);
            }
            else
            {
                GUILayout.Box("Field is null", GUILayout.Width(NitroxGUILayout.VALUE_WIDTH));
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Pressed Sprite", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (selectable.spriteState.pressedSprite)
            {
                ImageDrawer.DrawTexture(selectable.spriteState.pressedSprite.texture);
            }
            else
            {
                GUILayout.Box("Field is null", GUILayout.Width(NitroxGUILayout.VALUE_WIDTH));
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Selected Sprite", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (selectable.spriteState.selectedSprite)
            {
                ImageDrawer.DrawTexture(selectable.spriteState.selectedSprite.texture);
            }
            else
            {
                GUILayout.Box("Field is null", GUILayout.Width(NitroxGUILayout.VALUE_WIDTH));
            }
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Disabled Sprite", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            if (selectable.spriteState.disabledSprite)
            {
                ImageDrawer.DrawTexture(selectable.spriteState.disabledSprite.texture);
            }
            else
            {
                GUILayout.Box("Field is null", GUILayout.Width(NitroxGUILayout.VALUE_WIDTH));
            }
        }
    }

    private static void DrawTransitionAnimation(Selectable selectable)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Normal Trigger", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            selectable.animationTriggers.normalTrigger = GUILayout.TextField(selectable.animationTriggers.normalTrigger);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Highlighted Trigger", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            selectable.animationTriggers.highlightedTrigger = GUILayout.TextField(selectable.animationTriggers.highlightedTrigger);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Pressed Trigger", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            selectable.animationTriggers.pressedTrigger = GUILayout.TextField(selectable.animationTriggers.pressedTrigger);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Selected Trigger", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            selectable.animationTriggers.selectedTrigger = GUILayout.TextField(selectable.animationTriggers.selectedTrigger);
        }

        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Disabled Trigger", NitroxGUILayout.DrawerLabel, GUILayout.Width(NitroxGUILayout.DEFAULT_LABEL_WIDTH));
            NitroxGUILayout.Separator();
            selectable.animationTriggers.disabledTrigger = GUILayout.TextField(selectable.animationTriggers.disabledTrigger);
        }
    }
}
