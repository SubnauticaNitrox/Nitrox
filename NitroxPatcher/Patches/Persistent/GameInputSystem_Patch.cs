using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.MonoBehaviours.Gui.Input;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings;
using UnityEngine.InputSystem;

namespace NitroxPatcher.Patches.Persistent;

/// <summary>
/// Inserts Nitrox's keybinds in the new Subnautica input system
/// </summary>
public partial class GameInputSystem_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo INITIALIZE_METHOD = Reflect.Method((GameInputSystem t) => t.Initialize());
    private static readonly MethodInfo DEINITIALIZE_METHOD = Reflect.Method((GameInputSystem t) => t.Deinitialize());
    private static GameInput.Button[] oldAllActions;

    public static void InitializePrefix(GameInputSystem __instance)
    {
        int buttonId = KeyBindingManager.NITROX_BASE_ID;
        CachedEnumString<GameInput.Button> actionNames = GameInput.ActionNames;
        oldAllActions = GameInput.AllActions;
        FieldInfo allActionsField = typeof(GameInput).GetField(nameof(GameInput.AllActions), BindingFlags.Public | BindingFlags.Static);
        GameInput.Button[] allActions = [.. GameInput.AllActions, .. Enumerable.Range(buttonId, KeyBindingManager.KeyBindings.Count).Cast<GameInput.Button>()];
        allActionsField?.SetValue(null, allActions);

        foreach (KeyBinding keyBinding in KeyBindingManager.KeyBindings)
        {
            GameInput.Button button = (GameInput.Button)buttonId++;
            actionNames.valueToString[button] = keyBinding.ButtonLabel;

            if (!string.IsNullOrEmpty(keyBinding.DefaultKeyboardKey))
            {
                // See GameInputSystem.bindingsKeyboard definition
                GameInputSystem.bindingsKeyboard.Add(button, $"<Keyboard>/{keyBinding.DefaultKeyboardKey}");
            }
            if (!string.IsNullOrEmpty(keyBinding.DefaultControllerKey))
            {
                // See GameInputSystem.bindingsController definition
                GameInputSystem.bindingsController.Add(button, $"<Gamepad>/{keyBinding.DefaultControllerKey}");
            }
        }
    }

    public static void DeinitializePrefix()
    {
        FieldInfo allActionsField = typeof(GameInput).GetField(nameof(GameInput.AllActions), BindingFlags.Public | BindingFlags.Static);
        allActionsField?.SetValue(null, oldAllActions);
    }

    /*
     * Modifying the actions must happen before actionMapGameplay.Enable because that line is responsible
     * for activating the actions callback we'll be setting
     * 
     * GameInputSystem_Initialize_Patch.RegisterKeybindsActions(this); <--- [INSERTED LINE]
     * this.actionMapGameplay.Enable();
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchStartForward([
                                                new(OpCodes.Ldarg_0),
                                                new(OpCodes.Ldfld),
                                                new(OpCodes.Callvirt, Reflect.Method((InputActionMap t) => t.Enable()))
                                            ])
                                            .Insert([
                                                new CodeInstruction(OpCodes.Ldarg_0),
                                                new CodeInstruction(OpCodes.Call, Reflect.Method(() => RegisterKeybindsActions(default)))
                                            ])
                                            .InstructionEnumeration();
    }
    
    /// <summary>
    /// Set the actions callbacks for our own keybindings once they're actually created only
    /// </summary>
    public static void RegisterKeybindsActions(GameInputSystem gameInputSystem)
    {
        int buttonId = KeyBindingManager.NITROX_BASE_ID;
        foreach (KeyBinding keyBinding in KeyBindingManager.KeyBindings)
        {
            GameInput.Button button = (GameInput.Button)buttonId++;
            gameInputSystem.actions[button].started += keyBinding.Execute;
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, INITIALIZE_METHOD, ((Action<GameInputSystem>)InitializePrefix).Method);
        PatchPrefix(harmony, DEINITIALIZE_METHOD, ((Action)DeinitializePrefix).Method);
    }
}
