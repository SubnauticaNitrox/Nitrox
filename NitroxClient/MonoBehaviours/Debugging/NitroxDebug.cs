using JetBrains.Annotations;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours.Debugging
{
    public class NitroxDebug : MonoBehaviour
    {
        public KeyCode EnableDebuggerHotkey;
        public List<BaseDebugger> Debuggers;
        private HashSet<BaseDebugger> wereActive;
        private bool isDebugging;

        public void Awake()
        {
            EnableDebuggerHotkey = KeyCode.F7;
            wereActive = new HashSet<BaseDebugger>();
            Debuggers = new List<BaseDebugger>
            {
                gameObject.AddComponent<SceneDebugger>()
            };
        }

        public void OnGUI()
        {
            if (!isDebugging)
            {
                return;
            }
            
            CheckDebuggerHotkeys(Event.current);

            using (new GUILayout.AreaScope(new Rect(10, 10, 200, 40)))
            {
                using (new GUILayout.VerticalScope())
                {
                    foreach (BaseDebugger debugger in Debuggers)
                    {
                        debugger.enabled = GUILayout.Toggle(debugger.enabled, $"{debugger.DebuggerName} debugger ({debugger.GetHotkeyString()})");
                    }
                }
            }
        }

        public void Update()
        {
            if (Input.GetKeyDown(EnableDebuggerHotkey))
            {
                ToggleDebugging();
            }
        }

        public void ToggleDebugging()
        {
            isDebugging = !isDebugging;
            bool oldCursorState = Cursor.visible;
            if (isDebugging)
            {
                Cursor.visible = true;
                ShowDebuggers();
            }
            else
            {
                Cursor.visible = oldCursorState;
                HideDebuggers();   
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
