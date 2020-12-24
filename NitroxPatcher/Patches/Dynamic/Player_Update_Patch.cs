using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours.Gui.Input;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Player_Update_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Player);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(Player __instance)
        {
            // TODO: Use proper way to check if input is free, because players can be editing labels etc.
            if ((bool)((DevConsole)ReflectionHelper.ReflectionGet<DevConsole>(null, "instance", false, true)).ReflectionGet("state"))
            {
                return;
            }

            KeyBindingManager keyBindingManager = new KeyBindingManager();

            foreach (KeyBinding keyBinding in keyBindingManager.KeyboardKeyBindings)
            {
                bool isButtonDown = (bool)ReflectionHelper.ReflectionCall<GameInput>(null, "GetButtonDown", new Type[] { typeof(GameInput.Button) }, true, true, keyBinding.Button);

                if (isButtonDown)
                {
                    keyBinding.Action.Execute();
                }
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
