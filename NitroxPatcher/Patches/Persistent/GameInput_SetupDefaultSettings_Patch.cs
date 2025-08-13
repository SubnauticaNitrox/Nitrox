using System.Reflection;
using NitroxClient.MonoBehaviours.Gui.Input;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent;

public partial class GameInput_SetupDefaultSettings_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => GameInput.SetupDefaultSettings());

    public static void Postfix()
    {
        KeyBindingManager keyBindingManager = new();
        foreach (KeyBinding keyBinding in keyBindingManager.KeyboardKeyBindings)
        {
            GameInput.SetBinding(keyBinding.Device, keyBinding.Button, GameInput.BindingSet.Primary, keyBinding.PrimaryKey);

            // if the options.bin gets deleted (when loading SP) and the client.cfg has secondary keybinds, repopulate them.
            if (!string.IsNullOrEmpty(keyBinding.SecondaryKey))
            {
                GameInput.SetBinding(keyBinding.Device, keyBinding.Button, GameInput.BindingSet.Secondary, keyBinding.SecondaryKey);
            }
        }
    }
}
