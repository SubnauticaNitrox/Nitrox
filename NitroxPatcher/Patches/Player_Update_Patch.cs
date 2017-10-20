using Harmony;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.MonoBehaviours.Gui.Settings;
using NitroxModel.Helper;
using System;
using System.Reflection;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    public class Player_Update_Patch : NitroxPatch
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

            string keyBinding = SettingsManager.GetKey_Chat();
            if (keyBinding != "" || keyBinding != null || keyBinding != " ")
            {
                if (Input.GetKeyDown(keyBinding.ToLower()))
                {
                    PlayerChatManager chatManager = new PlayerChatManager();
                    chatManager.ShowChat();
                }
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
