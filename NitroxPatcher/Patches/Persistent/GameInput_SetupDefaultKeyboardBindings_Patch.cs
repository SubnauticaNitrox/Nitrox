using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours.Gui.Input;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings;
using NitroxClient.Serialization;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent;

public class GameInput_SetupDefaultKeyboardBindings_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => GameInput.SetupDefaultKeyboardBindings());

    public static void Postfix()
    {
        ClientConfig cfg = ClientConfig.InitClientConfig();
        KeyBindingManager keyBindingManager = new();
        foreach (KeyBinding keyBinding in keyBindingManager.KeyboardKeyBindings)
        {
            GameInput.SetBindingInternal(keyBinding.Device, keyBinding.Button, keyBinding.DefaultKeyBinding.BindingSet, keyBinding.DefaultKeyBinding.Binding);
            switch (keyBinding.HasSecondary) // if for some reason the options.bin gets deleted, such when loading sp, and the client.cfg has secondary keybinds, we need to repopulate them, this handles that.
            {
                case true when keyBinding.Button == (GameInput.Button)KeyBindingValues.CHAT:
                    GameInput.SetBindingInternal(keyBinding.Device, keyBinding.Button, GameInput.BindingSet.Secondary, cfg.OpenChatKeybindSecondary);
                    break;
                case true when keyBinding.Button == (GameInput.Button)KeyBindingValues.FOCUS_DISCORD:
                    GameInput.SetBindingInternal(keyBinding.Device, keyBinding.Button, GameInput.BindingSet.Secondary, cfg.FocusDiscordKeybindSecondary);
                    break;
            }
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
