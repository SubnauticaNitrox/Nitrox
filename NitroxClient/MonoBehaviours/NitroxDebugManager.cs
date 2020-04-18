using System.Collections.Generic;
using System.Linq;
using NitroxClient.Debuggers;
using NitroxModel.Core;
using NitroxModel.Logger;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours
{
    public class NitroxDebugManager : MonoBehaviour
    {
        public readonly List<BaseDebugger> Debuggers;
        public readonly KeyCode EnableDebuggerHotkey = KeyCode.F7;
        private readonly HashSet<BaseDebugger> prevActiveDebuggers = new HashSet<BaseDebugger>();
        private bool isDebugging;
        private Rect windowRect;

        private NitroxDebugManager()
        {
            Debuggers = NitroxServiceLocator.LocateServicePreLifetime<IEnumerable<BaseDebugger>>().ToList();
        }

        public static void ToggleCursor()
        {
            UWE.Utils.lockCursor = !UWE.Utils.lockCursor;
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

            if (isDebugging)
            {
                if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftControl))
                {
                    ToggleCursor();
                }

                CheckDebuggerHotkeys();

                foreach (BaseDebugger debugger in Debuggers)
                {
                    if (debugger.Enabled)
                    {
                        debugger.Update();
                    }
                }
            }
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
                foreach (BaseDebugger baseDebugger in Debuggers)
                {
                    baseDebugger.ResetWindowPosition();
                }
            }
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

        private void OnEnable()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            SceneManager.sceneUnloaded -= SceneManager_sceneUnloaded;
            SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            Log.Debug($"Scene '{scene.name}' loaded as {loadMode}");
        }

        private void SceneManager_sceneUnloaded(Scene scene)
        {
            Log.Debug($"Scene '{scene.name}' unloaded.");
        }

        private void SceneManager_activeSceneChanged(Scene fromScene, Scene toScene)
        {
            Log.Debug($"Active scene changed from '{fromScene.name}' to '{toScene.name}'");
        }
    }
}
