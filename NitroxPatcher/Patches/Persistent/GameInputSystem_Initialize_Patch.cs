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
///     Inserts Nitrox's keybinds in the new Subnautica input system.
///     Extends GameInput.AllActions so the game creates InputAction entries for Nitrox buttons,
///     which is required for compatibility with Nautilus when both mods run together.
/// </summary>
public class GameInputSystem_Initialize_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((GameInputSystem t) => t.Initialize());
    private static readonly MethodInfo DEINITIALIZE_METHOD = Reflect.Method((GameInputSystem t) => t.Deinitialize());
    private static GameInput.Button[] oldAllActions;

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD, ((Action<GameInputSystem>)Prefix).Method);
        PatchTranspiler(harmony, TARGET_METHOD, ((Func<IEnumerable<CodeInstruction>, IEnumerable<CodeInstruction>>)Transpiler).Method);
        PatchPrefix(harmony, DEINITIALIZE_METHOD, ((Action)DeinitializePrefix).Method);
    }

    /// <summary>
    ///     Set the actions callbacks for our own keybindings once they're actually created.
    ///     If the game didn't create entries (e.g. when AllActions extension doesn't apply to this game version),
    ///     we create and add the InputActions ourselves, matching Nautilus's approach.
    /// </summary>
    private static void RegisterKeybindsActions(GameInputSystem gameInputSystem)
    {
        int buttonId = KeyBindingManager.NITROX_BASE_ID;
        foreach (KeyBinding keyBinding in KeyBindingManager.KeyBindings)
        {
            GameInput.Button button = (GameInput.Button)buttonId++;
            if (!gameInputSystem.actions.TryGetValue(button, out InputAction action))
            {
                // Game didn't create this action (AllActions may not be used for action creation in this build).
                // Create and add it ourselves, matching Nautilus's InitializePostfix pattern.
                string buttonName = GameInput.ActionNames.valueToString.TryGetValue(button, out string name) ? name : button.ToString();
                action = new InputAction(buttonName, InputActionType.Button);
                if (!string.IsNullOrEmpty(keyBinding.DefaultKeyboardKey))
                {
                    action.AddBinding($"<Keyboard>/{keyBinding.DefaultKeyboardKey}");
                }
                if (!string.IsNullOrEmpty(keyBinding.DefaultControllerKey))
                {
                    action.AddBinding($"<Gamepad>/{keyBinding.DefaultControllerKey}");
                }
                gameInputSystem.actions[button] = action;
                action.started += gameInputSystem.OnActionStarted;
                action.Enable();
            }
            action.started += keyBinding.Execute;
        }
    }

    private static void Prefix(GameInputSystem __instance)
    {
        int buttonId = KeyBindingManager.NITROX_BASE_ID;
        oldAllActions = GameInput.AllActions;

        // Only extend AllActions if Nitrox buttons aren't already present.
        // AllActions is typically initialized from Enum.GetValues (patched by Enum_GetValues_Patch),
        // so it may already contain Nitrox buttons. Adding them again would cause duplicate keybinds
        // in the settings UI since the game builds the binding list from both sources.
        if (!GameInput.AllActions.Any(b => (int)b >= buttonId && (int)b < buttonId + KeyBindingManager.KeyBindings.Count))
        {
            GameInputAccessor.AllActions =
            [
                .. GameInput.AllActions,
                .. Enumerable.Range(buttonId, KeyBindingManager.KeyBindings.Count).Cast<GameInput.Button>()
            ];
        }

        CachedEnumString<GameInput.Button> actionNames = GameInput.ActionNames;

        // Access Language.main's internal strings dictionary so we can register "Option{buttonId}"
        // entries for the binding conflict dialog (the game formats conflict messages using
        // "Option" + button.ToString(), which yields "Option1000" for Nitrox buttons).
        Dictionary<string, string> langStrings = null;
        if (Language.main != null)
        {
            langStrings = (Dictionary<string, string>)AccessTools.Field(typeof(Language), "strings").GetValue(Language.main);
        }

        foreach (KeyBinding keyBinding in KeyBindingManager.KeyBindings)
        {
            GameInput.Button button = (GameInput.Button)buttonId++;
            actionNames.valueToString[button] = keyBinding.ButtonLabel;

            // Register the "Option1000" style key so the conflict dialog shows the translated label
            langStrings?.TryAdd($"Option{(int)button}", Language.main.Get(keyBinding.ButtonLabel));

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

    private static void DeinitializePrefix()
    {
        GameInputAccessor.AllActions = oldAllActions;
    }

    /*
     * Modifying the actions must happen before actionMapGameplay.Enable because that line is responsible
     * for activating the actions callback we'll be setting
     *
     * GameInputSystem_Initialize_Patch.RegisterKeybindsActions(this); <--- [INSERTED LINE]
     * this.actionMapGameplay.Enable();
     */
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).MatchStartForward(new(OpCodes.Ldarg_0), new(OpCodes.Ldfld), new(OpCodes.Callvirt, Reflect.Method((InputActionMap t) => t.Enable())))
                                            .Insert(new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Call, Reflect.Method(() => RegisterKeybindsActions(default))))
                                            .InstructionEnumeration();
    }

    private static class GameInputAccessor
    {
        /// <summary>
        ///     Required because "GameInput.AllActions" is a read only field.
        /// </summary>
        private static readonly FieldInfo allActionsField = Reflect.Field(() => GameInput.AllActions);

        public static GameInput.Button[] AllActions
        {
            set
            {
                allActionsField.SetValue(null, value);
            }
        }
    }
}
