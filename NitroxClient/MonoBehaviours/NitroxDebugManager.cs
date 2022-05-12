using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NitroxClient.Debuggers;
using NitroxModel.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours;

[ExcludeFromCodeCoverage]
public class NitroxDebugManager : MonoBehaviour
{
    private const KeyCode ENABLE_DEBUGGER_HOTKEY = KeyCode.F7;

    private List<BaseDebugger> debuggers;
    private readonly HashSet<BaseDebugger> prevActiveDebuggers = new();

    private bool showDebuggerList;
    private bool isDebugging;
    private Rect windowRect;

    private void Awake()
    {
        debuggers = NitroxServiceLocator.LocateServicePreLifetime<IEnumerable<BaseDebugger>>().ToList();
    }

    public static void ToggleCursor()
    {
        UWE.Utils.lockCursor = !UWE.Utils.lockCursor;
    }

    public void OnGUI()
    {
#if DEBUG
        if (!isDebugging)
        {
            return;
        }

        // Main window to display all available debuggers.
        windowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Keyboard), windowRect, DoWindow, "Nitrox debugging");

        // Render debugger windows if they are enabled.
        foreach (BaseDebugger debugger in debuggers)
        {
            debugger.OnGUI();
        }
#endif
    }

    public void Update()
    {
#if DEBUG
        if (Input.GetKeyDown(ENABLE_DEBUGGER_HOTKEY))
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

            foreach (BaseDebugger debugger in debuggers.Where(debugger => debugger.Enabled))
            {
                debugger.Update();
            }
        }
#endif
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
            foreach (BaseDebugger baseDebugger in debuggers)
            {
                baseDebugger.ResetWindowPosition();
            }
        }
    }

    private void DoWindow(int windowId)
    {
        using (new GUILayout.VerticalScope())
        {
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Toggle cursor (CTRL+C)"))
                {
                    ToggleCursor();
                }

                if (GUILayout.Button("Show / Hide", GUILayout.Width(100)))
                {
                    showDebuggerList = !showDebuggerList;
                    windowRect = default;
                }
            }
            if (showDebuggerList)
            {
                foreach (BaseDebugger debugger in debuggers)
                {
                    debugger.Enabled = GUILayout.Toggle(debugger.Enabled, $"{debugger.DebuggerName} debugger{debugger.HotkeyString}");
                }
            }
        }
    }

    private void CheckDebuggerHotkeys()
    {
        foreach (BaseDebugger debugger in debuggers)
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

    private static void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        Log.Debug($"Scene {scene.name} loaded as {loadMode}");
    }

    private static void SceneManager_sceneUnloaded(Scene scene)
    {
        Log.Debug($"Scene {scene.name} unloaded.");
    }

    private static void SceneManager_activeSceneChanged(Scene fromScene, Scene toScene)
    {
        Log.Debug($"Active scene changed from {fromScene.name} to {toScene.name}");
    }
}
