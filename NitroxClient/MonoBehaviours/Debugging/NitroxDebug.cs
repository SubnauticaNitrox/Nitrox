using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Debugging
{
    public class NitroxDebug : MonoBehaviour
    {
        public KeyCode EnableDebuggerHotkey;
        public List<BaseDebugger> Debuggers;
        private HashSet<BaseDebugger> wereActive;
        private bool isDebugging;
        private Rect windowRect;

        public void Awake()
        {
            windowRect = new Rect(10, 10, 200, 0);

            EnableDebuggerHotkey = KeyCode.F7;
            wereActive = new HashSet<BaseDebugger>();
            Debuggers = new List<BaseDebugger>
            {
                gameObject.AddComponent<SceneDebugger>(),
                gameObject.AddComponent<NetworkDebugger>()
            };
        }

        public void OnGUI()
        {
            if (!isDebugging)
            {
                return;
            }

            windowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Keyboard), windowRect, DoWindow, "Nitrox debugging");
        }

        public void Update()
        {
            if (Input.GetKeyDown(EnableDebuggerHotkey))
            {
                ToggleDebugging();
            }

            if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftControl))
            {
                ToggleCursor();
            }

            CheckDebuggerHotkeys();
        }

        public void ToggleDebugging()
        {
            isDebugging = !isDebugging;
            if (isDebugging)
            {
                UWE.Utils.PushLockCursor(false);
                ShowDebuggers();
            }
            else
            {
                UWE.Utils.PopLockCursor();
                HideDebuggers();
            }
        }

        public void ToggleCursor()
        {
            UWE.Utils.lockCursor = !UWE.Utils.lockCursor;
        }

        private void DoWindow(int windowId)
        {
            using (new GUILayout.VerticalScope(GUILayout.ExpandHeight(true)))
            {
                if (GUILayout.Button("Toggle cursor (CTRL+C)"))
                {
                    ToggleCursor();
                }

                foreach (BaseDebugger debugger in Debuggers)
                {
                    debugger.enabled = GUILayout.Toggle(debugger.enabled, $"{debugger.DebuggerName} debugger ({debugger.GetHotkeyString()})");
                }
            }
        }

        private void CheckDebuggerHotkeys()
        {
            if (!isDebugging)
            {
                return;
            }
            foreach (BaseDebugger debugger in Debuggers)
            {
                if (Input.GetKeyDown(debugger.Hotkey) && Input.GetKey(KeyCode.LeftControl) == debugger.HotkeyControlRequired && Input.GetKey(KeyCode.LeftShift) == debugger.HotkeyShiftRequired && Input.GetKey(KeyCode.LeftAlt) == debugger.HotkeyAltRequired)
                {
                    debugger.enabled = !debugger.enabled;
                }
            }
        }

        private void CheckDebuggerHotkeys(Event e)
        {
            if (!e.isKey || e.type != EventType.KeyDown)
            {
                return;
            }

            foreach (BaseDebugger debugger in Debuggers)
            {
                if (e.keyCode == debugger.Hotkey && e.control == debugger.HotkeyControlRequired && e.shift == debugger.HotkeyShiftRequired && e.alt == debugger.HotkeyAltRequired)
                {
                    debugger.enabled = !debugger.enabled;
                }
            }
        }

        private void HideDebuggers()
        {
            foreach (BaseDebugger debugger in GetComponents<BaseDebugger>())
            {
                if (debugger.enabled)
                {
                    wereActive.Add(debugger);
                }
                debugger.enabled = false;
            }
        }

        private void ShowDebuggers()
        {
            foreach (BaseDebugger debugger in wereActive)
            {
                debugger.enabled = true;
            }
            wereActive.Clear();
        }
    }
}
