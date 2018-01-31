using System.Collections.Generic;
using NitroxClient.Debuggers;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class NitroxDebugManager : MonoBehaviour
    {
        public readonly KeyCode EnableDebuggerHotkey = KeyCode.F7;
        public readonly List<BaseDebugger> Debuggers;
        private readonly HashSet<BaseDebugger> prevActiveDebuggers = new HashSet<BaseDebugger>();
        private bool isDebugging;
        private Rect windowRect;

        private NitroxDebugManager()
        {
            Debuggers = new List<BaseDebugger>
            {
                new SceneDebugger(),
                new NetworkDebugger()
            };
        }

        public void OnGUI()
        {
            if (!isDebugging)
            {
                return;
            }

            // Main window to display all available debuggers.
            windowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Keyboard), windowRect, DoWindow, "Nitrox debugging");

            // Render debugger windows if they are enabled.
            foreach (BaseDebugger debugger in Debuggers)
            {
                debugger.OnGUI();
            }
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
                    string hotkeyString = debugger.GetHotkeyString();
                    debugger.Enabled = GUILayout.Toggle(debugger.Enabled, $"{debugger.DebuggerName} debugger{(!string.IsNullOrEmpty(hotkeyString) ? $" ({hotkeyString})" : "")}");
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
                    debugger.Enabled = !debugger.Enabled;
                }
            }
        }

        private void HideDebuggers()
        {
            foreach (BaseDebugger debugger in GetComponents<BaseDebugger>())
            {
                if (debugger.Enabled)
                {
                    prevActiveDebuggers.Add(debugger);
                }
                debugger.Enabled = false;
            }
        }

        private void ShowDebuggers()
        {
            foreach (BaseDebugger debugger in prevActiveDebuggers)
            {
                debugger.Enabled = true;
            }
            prevActiveDebuggers.Clear();
        }
    }
}
