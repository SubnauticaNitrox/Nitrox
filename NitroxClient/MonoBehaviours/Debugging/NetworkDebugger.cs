using UnityEngine;

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
                        GUILayout.Label("TODO: In/out-coming packets log", "header");
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
