using NitroxModel.Logger;
using NitroxModel.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.Unity.Helper;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours.Debugging
{
    public class NetworkDebugger : BaseDebugger
    {
        public void Awake()
        {
            Hotkey = KeyCode.N;
            HotkeyControlRequired = true;
            WindowRect.width = 400;
            SkinCreationOptions = GUISkinCreationOptions.DERIVEDCOPY;

            Tabs.Add("Packets");
        }

        public override void DoWindow(int windowId)
        {
            switch (ActiveTab)
            {
                default:
                case 0:
                    using (new GUILayout.VerticalScope("Box"))
                    {
                        GUILayout.Label("TODO", "header");
                    }
                    break;
            }
        }

        protected override void OnSetSkin(GUISkin skin)
        {
            base.OnSetSkin(skin);
        }
    }
}
