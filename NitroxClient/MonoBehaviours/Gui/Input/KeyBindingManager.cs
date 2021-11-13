using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NitroxClient.GameLogic.ChatUI;
using NitroxModel.Core;
using NitroxModel.Logger;
using NitroxModel.Platforms.OS.Shared;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.Input
{
    public static class KeyBindingManager
    {
        public static List<KeyBinding> KeyboardBindings { get; } = new()
        {
            // new bindings should not be set to a value equivalent to a pre-existing GameInput.Button enum or another custom binding
            new KeyBinding("Chat", KeyCode.Y, () =>
            {
                // If no other UWE input field is currently active then allow chat to open.
                if (FPSInputModule.current.lastGroup == null)
                {
                    NitroxServiceLocator.LocateService<PlayerChatManager>().SelectChat();
                }
            }),
            new KeyBinding("Open log", KeyCode.L, () => FileSystem.Instance.OpenOrExecuteFile(Log.GetMostRecentLogFile()))
        };

        /// <summary>
        ///     Returns highest custom key binding value.
        /// </summary>
        [UsedImplicitly]
        public static int GetHighestKeyBindingValue()
        {
            return 44 + (KeyboardBindings?.Count ?? 0);
        }

        public class KeyBinding
        {
            /// <summary>
            ///     Index starts from 44 which is the last key binding defined by Subnautica.
            /// </summary>
            private static int index = 44;

            public GameInput.Device Device { get; }
            public string Label { get; }
            public KeyCode Default { get; }
            public Action Action { get; }
            public bool InGameOnly { get; }
            public GameInput.Button Button { get; }
            public KeyCode Current { get; set; }

            public KeyBinding(string buttonLabel, KeyCode @default, Action action, bool inGameOnly = true, GameInput.Device buttonDevice = GameInput.Device.Keyboard)
            {
                Button = (GameInput.Button)(++index);
                Device = buttonDevice;
                Label = buttonLabel;
                Action = action;
                Current = Default = @default;
                InGameOnly = inGameOnly;
            }
        }
    }
}