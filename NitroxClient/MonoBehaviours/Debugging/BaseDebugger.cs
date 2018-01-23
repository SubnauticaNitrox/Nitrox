using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Debugging
{
    public abstract class BaseDebugger : MonoBehaviour
    {
        public static GUISkin DefaultSkin;
        public Rect WindowRect;
        public bool CanDragWindow;
        public KeyCode Hotkey;
        public bool HotkeyControlRequired;
        public bool HotkeyShiftRequired;
        public bool HotkeyAltRequired;
        public string DebuggerName;
        public GUISkin Skin;

        protected BaseDebugger()
        {
            string name = GetType().Name;
            DebuggerName = name.Substring(0, name.IndexOf("Debugger"));

            WindowRect = new Rect(Screen.width / 2 - 100, 100, 200, 0);
            CanDragWindow = true;
            enabled = false;
        }

        private void DoWindowInternal(int windowId)
        {
            DoWindow(windowId);
            if (CanDragWindow)
            {
                GUI.DragWindow();
            }
        }

        private GUISkin GetDebuggerSkin()
        {
            if (Skin == null)
            {
                if (DefaultSkin == null)
                {
                    return GUI.skin;
                }
                return DefaultSkin;
            }
            return Skin;
        }

        protected virtual void OnGUI()
        {
            GUISkin oldSkin = GUI.skin;
            GUI.skin = GetDebuggerSkin();
            WindowRect = GUILayout.Window(0, WindowRect, DoWindowInternal, $"[DEBUGGER] {DebuggerName}", GUILayout.ExpandHeight(true));
            GUI.skin = oldSkin;
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
    }
}
