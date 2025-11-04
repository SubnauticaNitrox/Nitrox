using System;
using System.Collections.Generic;
using System.Globalization;
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
public partial class GameInputSystem_Initialize_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((GameInputSystem t) => t.Initialize());

    public static void Prefix(GameInputSystem __instance)
    {
        CachedEnumString<GameInput.Button> actionNames = GameInput.ActionNames;

        int buttonId = KeyBindingManager.NITROX_BASE_ID;
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
}
