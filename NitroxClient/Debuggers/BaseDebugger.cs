using System;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.Debuggers
{
    public abstract class BaseDebugger
    {
        public readonly string DebuggerName;
        public readonly KeyCode Hotkey;
        public readonly bool HotkeyAltRequired;
        public readonly bool HotkeyControlRequired;
        public readonly bool HotkeyShiftRequired;
        public readonly GUI_SkinCreationOptions SkinCreationOptions;

        /// <summary>
        /// Currently active tab. This is the index used with <see cref="tabs"/>.
        /// </summary>
        protected DebuggerTab activeTab;

        private readonly bool canDragWindow = true;

        public bool Enabled { get; set; }

        private Rect windowRect;

        /// <summary>
        /// Optional rendered tabs of the current debugger.
        /// </summary>
        /// <remarks>
        /// <see cref="activeTab"/> gives the index of the currently selected tab.
        /// </remarks>
        private readonly Dictionary<string, DebuggerTab> tabs = new Dictionary<string, DebuggerTab>();

        protected BaseDebugger(int desiredWidth, string debuggerName = null, KeyCode hotkey = KeyCode.None, bool control = false, bool shift = false, bool alt = false, GUI_SkinCreationOptions skinOptions = GUI_SkinCreationOptions.DEFAULT)
        {
            if (desiredWidth < 200)
            {
                desiredWidth = 200;
            }

            windowRect = new Rect(Screen.width / 2 - (desiredWidth / 2), 100, desiredWidth, 0); // Default position in center of screen.
            Hotkey = hotkey;
            HotkeyAltRequired = alt;
            HotkeyShiftRequired = shift;
            HotkeyControlRequired = control;
            SkinCreationOptions = skinOptions;

            if (string.IsNullOrEmpty(debuggerName))
            {
                string name = GetType().Name;
                DebuggerName = name.Substring(0, name.IndexOf("Debugger", StringComparison.Ordinal));
            }
            else
            {
                DebuggerName = debuggerName;
            }
        }

        public enum GUI_SkinCreationOptions
        {
            /// <summary>
            /// Uses the NitroxDebug skin.
            /// </summary>
            DEFAULT,

            /// <summary>
            /// Creates a copy of the default Unity IMGUI skin and sets the copied skin as render skin.
            /// </summary>
            UNITY_COPY,

            /// <summary>
            /// Creates a copy based on the NitroxDebug skin and sets the copied skin as render skin.
            /// </summary>
            DERIVED_COPY
        }

        public DebuggerTab AddTab(DebuggerTab tab)
        {
            Validate.NotNull(tab);

            tabs.Add(tab.Name, tab);
            return tab;
        }

        public DebuggerTab AddTab(string name, Action render)
        {
            return AddTab(new DebuggerTab(name, render));
        }

        public string GetHotkeyString()
        {
            if (Hotkey == KeyCode.None)
            {
                return "";
            }

            return $"{(HotkeyControlRequired ? "CTRL+" : "")}{(HotkeyAltRequired ? "ALT+" : "")}{(HotkeyShiftRequired ? "SHIFT+" : "")}{Hotkey}";
        }

        public Optional<DebuggerTab> GetTab(string name)
        {
            Validate.NotNull(name);

            tabs.TryGetValue(name, out DebuggerTab tab);
            return Optional.OfNullable(tab);
        }

        public virtual void Update()
        {
            // Defaults to a no-op but can be overriden
        }

        /// <summary>
        /// Call this inside a <see cref="MonoBehaviour.OnGUI"/> method.
        /// </summary>
        public virtual void OnGUI()
        {
            if (!Enabled)
            {
                return;
            }

            GUISkin skin = GetSkin();
            GUISkinUtils.RenderWithSkin(skin, () =>
            {
                windowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Keyboard), windowRect, RenderInternal, $"[DEBUGGER] {DebuggerName}", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            });
        }

        /// <summary>
        /// Optionally adjust the skin that is used during render.
        /// </summary>
        /// <remarks>
        /// Set <see cref="SkinCreationOptions"/> on <see cref="GUI_SkinCreationOptions.UNITY_COPY"/> or <see cref="GUI_SkinCreationOptions.DERIVED_COPY"/> in constructor before using this method.
        /// </remarks>
        /// <param name="skin">Skin that is being used during <see cref="Render(int)"/>.</param>
        protected virtual void OnSetSkin(GUISkin skin)
        {
        }

        /// <summary>
        /// Optionally use a custom render solution for the debugger by overriding this method.
        /// </summary>
        protected virtual void Render()
        {
            activeTab.Render?.Invoke();
        }

        /// <summary>
        /// Gets (a copy of) a skin specified by <see cref="GUI_SkinCreationOptions"/>.
        /// </summary>
        /// <returns>A reference to an existing or copied skin.</returns>
        private GUISkin GetSkin()
        {
            GUISkin skin = GUI.skin;
            string skinName = GetSkinName();
            switch (SkinCreationOptions)
            {
                case GUI_SkinCreationOptions.DEFAULT:
                    skin = GUISkinUtils.RegisterDerivedOnce("debuggers.default", s =>
                    {
                        SetBaseStyle(s);
                        OnSetSkinImpl(s);
                    });
                    break;

                case GUI_SkinCreationOptions.UNITY_COPY:
                    skin = GUISkinUtils.RegisterDerivedOnce(skinName, OnSetSkinImpl);
                    break;

                case GUI_SkinCreationOptions.DERIVED_COPY:
                    GUISkin baseSkin = GUISkinUtils.RegisterDerivedOnce("debuggers.default", SetBaseStyle);
                    skin = GUISkinUtils.RegisterDerivedOnce(skinName, OnSetSkinImpl, baseSkin);
                    break;
            }

            return skin;
        }

        private string GetSkinName()
        {
            string name = GetType().Name;
            return $"debuggers.{name.Substring(0, name.IndexOf("Debugger", StringComparison.Ordinal)).ToLowerInvariant()}";
        }

        private void OnSetSkinImpl(GUISkin skin)
        {
            if (SkinCreationOptions == GUI_SkinCreationOptions.DEFAULT)
            {
                Enabled = false;
                throw new NotSupportedException($"Cannot change {nameof(GUISkin)} for {GetType().FullName} when accessing the default skin. Change {nameof(SkinCreationOptions)} to something else than {nameof(GUI_SkinCreationOptions.DEFAULT)}.");
            }

            OnSetSkin(skin);
        }

        private void RenderInternal(int windowId)
        {
            using (new GUILayout.HorizontalScope("Box"))
            {
                if (tabs.Count == 1)
                {
                    GUILayout.Label(tabs.First().Key, "tabActive");
                }
                else
                {
                    foreach (DebuggerTab tab in tabs.Values)
                    {
                        if (GUILayout.Button(tab.Name, activeTab == tab ? "tabActive" : "tab"))
                        {
                            activeTab = tab;
                        }
                    }
                }
            }

            Render();
            if (canDragWindow)
            {
                GUI.DragWindow();
            }
        }

        private void SetBaseStyle(GUISkin skin)
        {
            skin.label.alignment = TextAnchor.MiddleLeft;
            skin.label.margin = new RectOffset();
            skin.label.padding = new RectOffset();

            skin.SetCustomStyle("header", skin.label, s =>
            {
                s.margin.top = 10;
                s.margin.bottom = 10;
                s.alignment = TextAnchor.MiddleCenter;
                s.fontSize = 16;
                s.fontStyle = FontStyle.Bold;
            });

            skin.SetCustomStyle("tab", skin.button, s =>
            {
                s.fontSize = 16;
                s.margin = new RectOffset(5, 5, 5, 5);
            });

            skin.SetCustomStyle("tabActive", skin.button, s =>
            {
                s.fontStyle = FontStyle.Bold;
                s.fontSize = 16;
            });
        }

        public void ResetWindowPosition()
        {
            windowRect = new Rect(Screen.width / 2 - (windowRect.width / 2), 100, windowRect.width, windowRect.height); //Reset position of debuggers because SN sometimes throws the windows from planet 4546B
        }

        public class DebuggerTab
        {
            public DebuggerTab(string name, Action render)
            {
                Validate.NotNull(name, $"Expected a name for the {nameof(DebuggerTab)}");
                
                Name = name;
                Render = render;
            }

            public string Name { get; }
            public Action Render { get; }
        }
    }
}
