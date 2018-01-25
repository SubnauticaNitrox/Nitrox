using System;
using System.Collections.Generic;
using NitroxClient.Unity.Helper;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Debugging
{
    public abstract class BaseDebugger : MonoBehaviour
    {
        public Rect WindowRect;
        public bool CanDragWindow;
        public KeyCode Hotkey;
        public bool HotkeyControlRequired;
        public bool HotkeyShiftRequired;
        public bool HotkeyAltRequired;
        public string DebuggerName;
        public GUISkinCreationOptions SkinCreationOptions;
        public List<string> Tabs;
        public int ActiveTab;

        protected BaseDebugger()
        {
            string name = GetType().Name;
            DebuggerName = name.Substring(0, name.IndexOf("Debugger"));

            WindowRect = new Rect(Screen.width / 2 - 100, 100, 200, 0);
            CanDragWindow = true;
            SkinCreationOptions = GUISkinCreationOptions.DEFAULT;
            enabled = false;

            Tabs = new List<string>();
        }

        private void DoWindowInternal(int windowId)
        {
            using (new GUILayout.HorizontalScope("Box"))
            {
                if (Tabs.Count == 1)
                {
                    GUILayout.Label(Tabs[0], "tabActive");
                }
                else
                {
                    for (int i = 0; i < Tabs.Count; i++)
                    {
                        if (GUILayout.Button(Tabs[i], ActiveTab == i ? "tabActive" : "tab"))
                        {
                            ActiveTab = i;
                        }
                    }
                }
            }

            DoWindow(windowId);
            if (CanDragWindow)
            {
                GUI.DragWindow();
            }
        }

        private void OnGUIImpl()
        {
            WindowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Keyboard), WindowRect, DoWindowInternal, $"[DEBUGGER] {DebuggerName}", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        }

        private void OnGUI()
        {
            GUISkin skin = GUI.skin;
            string skinName = GetSkinName();
            switch (SkinCreationOptions)
            {
                case GUISkinCreationOptions.DEFAULT:
                    skin = GUISkinUtils.RegisterDerivedOnce("debuggers.default", s =>
                    {
                        SetBaseStyle(s);
                        OnSetSkinImpl(s);
                    });
                    break;

                case GUISkinCreationOptions.UNITYCOPY:
                    skin = GUISkinUtils.RegisterDerivedOnce(skinName, OnSetSkinImpl);
                    break;

                case GUISkinCreationOptions.DERIVEDCOPY:
                    GUISkin baseSkin = GUISkinUtils.RegisterDerivedOnce("debuggers.default", SetBaseStyle);
                    skin = GUISkinUtils.RegisterDerivedOnce(skinName, OnSetSkinImpl, baseSkin);
                    break;
            }
            GUISkinUtils.SwitchSkin(skin, OnGUIImpl);
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

        private void OnSetSkinImpl(GUISkin skin)
        {
            if (SkinCreationOptions == GUISkinCreationOptions.DEFAULT)
            {
                enabled = false;
                throw new NotSupportedException($"Cannot change {nameof(GUISkin)} for {GetType().FullName} when accessing the default skin. Change {nameof(SkinCreationOptions)} to something else than {nameof(GUISkinCreationOptions.DEFAULT)}.");
            }
            OnSetSkin(skin);
        }

        private string GetSkinName()
        {
            string name = GetType().Name;
            return $"debuggers.{name.Substring(0, name.IndexOf("Debugger")).ToLowerInvariant()}";
        }

        protected virtual void OnSetSkin(GUISkin skin)
        {
        }

        public abstract void DoWindow(int windowId);

        public string GetHotkeyString()
        {
            if (Hotkey == KeyCode.None)
            {
                return "";
            }
            return $"{(HotkeyControlRequired ? "CTRL+" : "")}{(HotkeyAltRequired ? "ALT+" : "")}{(HotkeyShiftRequired ? "SHIFT+" : "")}{Hotkey}";
        }

        public enum GUISkinCreationOptions
        {
            DEFAULT,
            UNITYCOPY,
            DERIVEDCOPY
        }
    }
}
