#if DEBUG
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NitroxClient.Debuggers;
using Nitrox.Model.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours;

[ExcludeFromCodeCoverage]
public class NitroxDebugManager : MonoBehaviour
{
    private const int NITROX_DEBUGGER_WINDOW_ID = 420;

    private const KeyCode ENABLE_DEBUGGER_HOTKEY = KeyCode.F7;

    private readonly HashSet<AbstractDebugger> prevActiveDebuggers = [];
    private List<AbstractDebugger> debuggers;

    private bool showDebuggerList = true;
    private bool isDebugging;
    private Rect windowRect;

    private void Awake()
    {
        debuggers = NitroxServiceLocator.LocateServicePreLifetime<IEnumerable<AbstractDebugger>>().ToList();
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
        windowRect = GUILayout.Window(NITROX_DEBUGGER_WINDOW_ID, windowRect, DoWindow, "Nitrox Debugging");

        // Render debugger windows if they are enabled.
        foreach (AbstractDebugger debugger in debuggers)
        {
            debugger.OnGUI();
        }

    }

    public void Update()
    {
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

            if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftControl))
            {
                ResetDebuggers();
            }

            CheckDebuggerHotkeys();
        }
    }

    public void ToggleDebugging()
    {
        isDebugging = !isDebugging;
        if (isDebugging)
        {
            ShowDebuggers();
            UWE.Utils.alwaysLockCursor = false;
            UWE.Utils.lockCursor = false;
        }
        else
        {
            UWE.Utils.lockCursor = true;
            HideDebuggers();
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

                if (GUILayout.Button("Reset (CTRL+R)"))
                {
                    ResetDebuggers();
                }

                if (GUILayout.Button("Show / Hide", GUILayout.Width(100)))
                {
                    showDebuggerList = !showDebuggerList;
                    windowRect = default;
                }
            }
            if (showDebuggerList)
            {
                foreach (AbstractDebugger debugger in debuggers)
                {
                    debugger.Enabled = GUILayout.Toggle(debugger.Enabled, $"{debugger.DebuggerName} debugger ({debugger.HotkeyString})");
                }
            }
        }
    }

    private void CheckDebuggerHotkeys()
    {
        foreach (AbstractDebugger debugger in debuggers)
        {
            if (Input.GetKeyDown(debugger.Hotkey) && Input.GetKey(KeyCode.LeftControl) == debugger.HotkeyControlRequired && Input.GetKey(KeyCode.LeftShift) == debugger.HotkeyShiftRequired && Input.GetKey(KeyCode.LeftAlt) == debugger.HotkeyAltRequired)
            {
                debugger.Enabled = !debugger.Enabled;
            }
        }
    }

    private void ResetDebuggers()
    {
        foreach (AbstractDebugger debugger in debuggers)
        {
            debugger.ResetWindowPosition();
        }
    }

    private void HideDebuggers()
    {
        foreach (AbstractDebugger debugger in GetComponents<AbstractDebugger>())
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
        foreach (AbstractDebugger debugger in prevActiveDebuggers)
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
#endif
