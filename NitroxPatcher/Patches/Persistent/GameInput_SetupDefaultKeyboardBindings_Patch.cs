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
        ClientConfig cfg = ClientConfig.Init();
        KeyBindingManager keyBindingManager = new();
        foreach (KeyBinding keyBinding in keyBindingManager.KeyboardKeyBindings)
        {
            GameInput.SetBindingInternal(keyBinding.Device, keyBinding.Button, GameInput.BindingSet.Primary, keyBinding.PrimaryKey);

            // if the options.bin gets deleted (when loading SP) and the client.cfg has secondary keybinds, repopulate them.
            switch ((KeyBindingValues)keyBinding.Button) 
            {
                case KeyBindingValues.CHAT:
                    GameInput.SetBindingInternal(keyBinding.Device, keyBinding.Button, GameInput.BindingSet.Secondary, cfg.OpenChatKeybindSecondary);
                    break;
                case KeyBindingValues.FOCUS_DISCORD:
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
