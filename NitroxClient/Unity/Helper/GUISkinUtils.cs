using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.Unity.Helper
{
    public static class GUISkinUtils
    {
        private static Dictionary<string, GUISkin> guiSkins = new Dictionary<string, GUISkin>();

        public static GUISkin CreateDerived(GUISkin baseSkin = null, string name = null)
        {
            if (baseSkin == null)
            {
                GUISkin prevSkin = GUI.skin;
                GUI.skin = null;
                baseSkin = GUI.skin;
                GUI.skin = prevSkin;
            }

            GUISkin copy = ScriptableObject.CreateInstance<GUISkin>();

            copy.name = name ?? Guid.NewGuid().ToString();
            copy.box = new GUIStyle(baseSkin.box);
            copy.button = new GUIStyle(baseSkin.button);
            copy.label = new GUIStyle(baseSkin.label);
            copy.scrollView = new GUIStyle(baseSkin.scrollView);
            copy.textArea = new GUIStyle(baseSkin.textArea);
            copy.textField = new GUIStyle(baseSkin.textField);
            copy.toggle = new GUIStyle(baseSkin.toggle);
            copy.window = new GUIStyle(baseSkin.window);
            copy.horizontalScrollbar = new GUIStyle(baseSkin.horizontalScrollbar);
            copy.horizontalScrollbarLeftButton = new GUIStyle(baseSkin.horizontalScrollbarLeftButton);
            copy.horizontalScrollbarRightButton = new GUIStyle(baseSkin.horizontalScrollbarRightButton);
            copy.horizontalScrollbarThumb = new GUIStyle(baseSkin.horizontalScrollbarThumb);
            copy.horizontalSlider = new GUIStyle(baseSkin.horizontalSlider);
            copy.horizontalSliderThumb = new GUIStyle(baseSkin.horizontalSliderThumb);
            copy.verticalScrollbar = new GUIStyle(baseSkin.verticalScrollbar);
            copy.verticalScrollbarDownButton = new GUIStyle(baseSkin.verticalScrollbarDownButton);
            copy.verticalScrollbarThumb = new GUIStyle(baseSkin.verticalScrollbarThumb);
            copy.verticalScrollbarUpButton = new GUIStyle(baseSkin.verticalScrollbarUpButton);
            copy.verticalSlider = new GUIStyle(baseSkin.verticalSlider);
            copy.verticalSliderThumb = new GUIStyle(baseSkin.verticalSliderThumb);
            copy.customStyles = baseSkin.customStyles.Select(s => new GUIStyle(s)).ToArray();
            copy.hideFlags = baseSkin.hideFlags;

            copy.settings.cursorColor = baseSkin.settings.cursorColor;
            copy.settings.cursorFlashSpeed = baseSkin.settings.cursorFlashSpeed;
            copy.settings.doubleClickSelectsWord = baseSkin.settings.doubleClickSelectsWord;
            copy.settings.tripleClickSelectsLine = baseSkin.settings.tripleClickSelectsLine;
            copy.settings.selectionColor = baseSkin.settings.selectionColor;

            // TODO: Create identical copy of font.
            copy.font = baseSkin.font; // Gives a giberish font: copy.font = UnityEngine.Object.Instantiate(baseSkin.font);

            return copy;
        }

        public static GUISkin RegisterDerived(string name, GUISkin baseSkin = null)
        {
            Validate.NotNull(name);
            Validate.IsFalse(guiSkins.ContainsKey(name), $"Name of new GUISkin already exists.");

            GUISkin newSkin = CreateDerived(baseSkin, name);
            guiSkins.Add(name, newSkin);
            return newSkin;
        }

        /// <summary>
        /// Creates a new default Unity skin if there are no existing skins with the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name for the new skin.</param>
        /// <param name="skinInitializer">Optional skin initializer.</param>
        /// <param name="baseSkin">Optional base skin to copy from.</param>
        /// <returns>New or cached skin.</returns>
        public static GUISkin RegisterDerivedOnce(string name, Action<GUISkin> skinInitializer = null, GUISkin baseSkin = null)
        {
            if (!guiSkins.ContainsKey(name))
            {
                GUISkin newSkin = RegisterDerived(name, baseSkin);
                skinInitializer?.Invoke(newSkin);
            }

            return guiSkins[name];
        }

        /// <summary>
        /// Switches the active skin until after the <paramref name="render"/> action.
        /// </summary>
        /// <param name="skin">Skin to switch to.</param>
        /// <param name="render">Render function to run.</param>
        public static void RenderWithSkin(GUISkin skin, Action render)
        {
            Validate.NotNull(render);

            GUISkin prevSkin = GUI.skin;
            GUI.skin = skin;
            render();
            GUI.skin = prevSkin;
        }

        /// <summary>
        /// Adds or sets a custom style to the skin.
        /// </summary>
        /// <remarks>
        /// Custom skins can be used by passing the "nameofskin" to the <see cref="GUIStyle"/> parameter of <see cref="GUILayout.Label(string, GUIStyle, GUILayoutOption[])"/> and similar.
        /// </remarks>
        /// <param name="skin">Skin to add a custom style to.</param>
        /// <param name="name">Name of the new custom style.</param>
        /// <param name="baseStyle">Style to base your custom style on.</param>
        /// <param name="modify">Function that changes the custom style to your liking.</param>
        public static void SetCustomStyle(this GUISkin skin, string name, GUIStyle baseStyle, Action<GUIStyle> modify)
        {
            GUIStyle style = new GUIStyle(baseStyle);
            style.name = name;
            modify(style);

            int index = Array.FindIndex(skin.customStyles, item => item.name == style.name);
            if (index >= 0)
            {
                skin.customStyles[index] = style;
            }
            else
            {
                // Increase array size and add style.
                List<GUIStyle> styles = new List<GUIStyle>(skin.customStyles)
                {
                    style
                };
                skin.customStyles = styles.ToArray();
            }
        }
    }
}
