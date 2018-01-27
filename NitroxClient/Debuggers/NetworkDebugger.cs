using UnityEngine;

namespace NitroxClient.Debuggers
{
    public class NetworkDebugger : BaseDebugger
    {
        public NetworkDebugger() : base(400, null, KeyCode.N, true, false, false, GUISkinCreationOptions.DERIVEDCOPY)
        {
            ActiveTab = AddTab("Packets", RenderTabPackets);
        }

        private void RenderTabPackets()
        {
            using (new GUILayout.VerticalScope("Box"))
            {
                GUILayout.Label("TODO: In/out-coming packets log", "header");
            }
        }
    }
}
